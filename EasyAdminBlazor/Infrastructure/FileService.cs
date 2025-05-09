using EasyAdminBlazor.Infrastructure.Encrypt;
using BootstrapBlazor.Components;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;
using Image = SixLabors.ImageSharp.Image;

namespace EasyAdminBlazor.Services;

public class FileService
{
    private readonly IBaseRepository<SysFile,long> _fileRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string[] IncludeExtension = Array.Empty<string>();
    private readonly string[] ExcludeExtension = [".exe", ".dll", ".jar", ".php", ".aspx"];
    private readonly long MaxSize = 104857600;
    private readonly string DirectoryName = "uploads";
    private readonly string DateTimeDirectory = "yyyy/MM/dd";

    public FileService(IBaseRepository<SysFile, long> fileRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _fileRepository = fileRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// 获取上传目录的所有子目录
    /// </summary>
    /// <returns></returns>
    public List<TreeViewItem<string>> GetGroups()
    {
        var env = _webHostEnvironment;
        var fileDirectory = Path.Combine(env.WebRootPath, DirectoryName).ToPath();
        var result = new List<TreeViewItem<string>>() { new TreeViewItem<string>("所有文件") { Text= "所有文件" } };

        foreach (var item in Directory.GetDirectories(fileDirectory)) {
            var dir = item.Replace(Path.Combine(env.WebRootPath, DirectoryName).ToPath(), "");
            result.Add(new TreeViewItem<string>(dir) { Text= dir });
        }

        return result;
    }

    /// <summary>
    /// 添加分组
    /// </summary>
    /// <param name="groupName"></param>
    public void AddGroup(string groupName) {
        var env = _webHostEnvironment;

        string fileDirectory = DirectoryName;
        fileDirectory = Path.Combine(env.WebRootPath, fileDirectory, groupName).ToPath();
        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="fileDirectory">文件目录</param>
    /// <param name="fileReName">文件重命名</param>
    /// <returns></returns>
    public async Task<SysFile> UploadFileAsync(UploadFile file, string fileDirectory = "", bool fileReName = true)
    {
        if (fileDirectory == "所有文件") fileDirectory = "";

        var extention = file.GetExtension()?.ToLower();
        var hasIncludeExtension = IncludeExtension?.Length > 0;
        if (hasIncludeExtension && !IncludeExtension.Contains(extention))
        {
            throw new Exception($"不允许上传{extention}文件格式");
        }
        var hasExcludeExtension = ExcludeExtension?.Length > 0;
        if (hasExcludeExtension && ExcludeExtension.Contains(extention))
        {
            throw new Exception($"不允许上传{extention}文件格式");
        }

        var fileLenth = file.Size;
        if (fileLenth > MaxSize)
        {
            throw new Exception($"文件大小不能超过{new FileSize(MaxSize)}");
        }

        var enableMd5 = false;
        var md5 = string.Empty;
        if (enableMd5)
        {
            md5 = Md5Encrypt.GetHash(file.File.OpenReadStream());
            var md5SysFile = await _fileRepository.Where(a => a.Md5 == md5).FirstAsync();
            if (md5SysFile != null)
            {
                var sameSysFile = new SysFile
                {
                    Provider = md5SysFile.Provider,
                    BucketName = md5SysFile.BucketName,
                    FileGuid = FreeUtil.NewMongodbId(),
                    SaveFileName = md5SysFile.SaveFileName,
                    FileName = file.OriginFileName,
                    Extension = extention,
                    FileDirectory = md5SysFile.FileDirectory,
                    Size = md5SysFile.Size,
                    SizeFormat = md5SysFile.SizeFormat,
                    LinkUrl = md5SysFile.LinkUrl,
                    Md5 = md5,
                };
                sameSysFile = await _fileRepository.InsertAsync(sameSysFile);
                return sameSysFile;
            }
        }

        if (fileDirectory.IsNull())
        {
            fileDirectory = DirectoryName;
            if (DateTimeDirectory.NotNull())
            {
                fileDirectory = Path.Combine(fileDirectory, DateTime.Now.ToString(DateTimeDirectory)).ToPath();
            }
        }
        else
        {
            fileDirectory = Path.Combine(DirectoryName, fileDirectory.Replace("\\","").Replace("/", "")).ToPath();
        }

        var fileSize = new FileSize(fileLenth);
        var SysFile = new SysFile
        {
            Provider = "local",
            BucketName = "",
            FileGuid = FreeUtil.NewMongodbId(),
            FileName = file.OriginFileName,
            Extension = extention,
            FileDirectory = fileDirectory,
            Size = fileSize.Size,
            SizeFormat = fileSize.ToString(),
            Md5 = md5
        };
        SysFile.SaveFileName = fileReName ? SysFile.FileGuid.ToString() : SysFile.FileName;

        var filePath = Path.Combine(fileDirectory, SysFile.SaveFileName + (fileReName ? file.GetExtension().IsImage() ? ".jpeg":SysFile.Extension :"")).ToPath();
        var url = string.Empty;
        SysFile.LinkUrl = $"/{filePath}";

        var env = _webHostEnvironment;
        fileDirectory = Path.Combine(env.WebRootPath, fileDirectory).ToPath();
        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }
        filePath = Path.Combine(env.WebRootPath, filePath).ToPath();
        //保存到磁盘
        await file.SaveToFileAsync(filePath, MaxSize);

        //如果是图片则压缩
        if (file.GetExtension().IsImage())
        {
            // 加载图像  
            using (var image = Image.Load(filePath))
            {
                var w = image.Width;
                var h = image.Height;

                if (w > 1600)
                {
                    // 调整图像大小  
                    image.Mutate(x => x.Resize(1600, 0, true));

                    var encoder = new JpegEncoder
                    {
                        Quality = 90
                    };

                    // 覆盖原图  
                    await image.SaveAsJpegAsync(filePath, encoder);

                    fileSize = new FileSize(new FileInfo(filePath).Length);
                    SysFile.Size = fileSize.Size;
                    SysFile.SizeFormat = fileSize.ToString();
                }
            }
        }

        if (await _fileRepository.Select.AnyAsync(x=>x.FileName == SysFile.FileName))
        {
            SysFile.FileName = SysFile.FileName.Replace(".", "_" + new Random().Next(1000, 9999)+".");
        }

        SysFile = await _fileRepository.InsertAsync(SysFile);

        return SysFile;
    }

    /// <summary>
    /// 上传多文件
    /// </summary>
    /// <param name="files">文件列表</param>
    /// <param name="fileDirectory">文件目录</param>
    /// <param name="fileReName">文件重命名</param>
    /// <returns></returns>
    public async Task<List<SysFile>> UploadFilesAsync(List<UploadFile> files, string fileDirectory = "", bool fileReName = true)
    {
        var fileList = new List<SysFile>();
        foreach (var file in files)
        {
            fileList.Add(await UploadFileAsync(file, fileDirectory, fileReName));
        }
        return fileList;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task DeleteAsync(long id)
    {
        var file = await _fileRepository.GetAsync(id);
        if (file == null)
        {
            return;
        }

        var env = _webHostEnvironment;
        var filePath = Path.Combine(env.WebRootPath, file.FileDirectory, file.SaveFileName.Replace(file.Extension, "") + file.Extension).ToPath();
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        await _fileRepository.DeleteAsync(file.Id);
    }
}

/// <summary>
/// 文件大小
/// </summary>
public struct FileSize
{
    /// <summary>
    /// 初始化文件大小
    /// </summary>
    /// <param name="size">文件大小</param>
    /// <param name="unit">文件大小单位</param>
    public FileSize(long size, FileSizeUnit unit = FileSizeUnit.Byte)
    {
        switch (unit)
        {
            case FileSizeUnit.K:
                Size = size * 1024; break;
            case FileSizeUnit.M:
                Size = size * 1024 * 1024; break;
            case FileSizeUnit.G:
                Size = size * 1024 * 1024 * 1024; break;
            default:
                Size = size; break;
        }
    }

    /// <summary>
    /// 文件字节长度
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// 获取文件大小，单位：字节
    /// </summary>
    public long GetSize()
    {
        return Size;
    }

    /// <summary>
    /// 获取文件大小，单位：K
    /// </summary>
    public double GetSizeByK()
    {
        return (Size / 1024.0).ToDouble(2);
    }

    /// <summary>
    /// 获取文件大小，单位：M
    /// </summary>
    public double GetSizeByM()
    {
        return (Size / 1024.0 / 1024.0).ToDouble(2);
    }

    /// <summary>
    /// 获取文件大小，单位：G
    /// </summary>
    public double GetSizeByG()
    {
        return (Size / 1024.0 / 1024.0 / 1024.0).ToDouble(2);
    }

    /// <summary>
    /// 输出描述
    /// </summary>
    public override string ToString()
    {
        if (Size >= 1024 * 1024 * 1024)
            return $"{GetSizeByG()} {FileSizeUnit.G.ToDescription()}";
        if (Size >= 1024 * 1024)
            return $"{GetSizeByM()} {FileSizeUnit.M.ToDescription()}";
        if (Size >= 1024)
            return $"{GetSizeByK()} {FileSizeUnit.K.ToDescription()}";
        return $"{Size} {FileSizeUnit.Byte.ToDescription()}";
    }
}

/// <summary>
/// 文件大小单位
/// </summary>
public enum FileSizeUnit
{
    /// <summary>
    /// 字节
    /// </summary>
    [Description("B")]
    Byte,

    /// <summary>
    /// K字节
    /// </summary>
    [Description("KB")]
    K,

    /// <summary>
    /// M字节
    /// </summary>
    [Description("MB")]
    M,

    /// <summary>
    /// G字节
    /// </summary>
    [Description("GB")]
    G
}
