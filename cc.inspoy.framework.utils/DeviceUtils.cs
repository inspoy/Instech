/**
 * == Inspoy Technology ==
 * Assembly: Instech.Framework.Utils
 * FileName: DeviceUtils.cs
 * Created on 2019/12/25 by inspoy
 * All rights reserved.
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Instech.Framework.Utils
{
    /// <summary>
    /// 硬件环境信息
    /// </summary>
    public class HardwareInfo
    {
        /// <summary>
        /// 硬件型号，品牌机为电脑型号，组装机为主板型号
        /// </summary>
        public string DeviceModel;

        /// <summary>
        /// 计算机名
        /// </summary>
        public string DeviceName;

        /// <summary>
        /// 硬件唯一ID
        /// </summary>
        public string UniqueDeviceId;

        /// <summary>
        /// CPU型号
        /// </summary>
        public string CpuModel;

        /// <summary>
        /// CPU逻辑线程数
        /// </summary>
        public int CpuCores;

        /// <summary>
        /// CPU主频
        /// </summary>
        public int CpuFrequency;

        /// <summary>
        /// 内存容量，单位兆
        /// </summary>
        public int MemorySize;

        /// <summary>
        /// 显卡型号
        /// </summary>
        public string GraphicCard;

        /// <summary>
        /// 显存容量，单位兆
        /// </summary>
        public int VideoMemorySize;
    }

    /// <summary>
    /// 软件环境信息
    /// </summary>
    public class SoftwareInfo
    {
        /// <summary>
        /// 当前运行平台
        /// </summary>
        public RuntimePlatform Platform;

        /// <summary>
        /// 网络连接状态
        /// </summary>
        public bool NetworkConnected;

        /// <summary>
        /// 操作系统名称
        /// </summary>
        public string OsName;

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public Version OsVersion;

        /// <summary>
        /// 系统语言
        /// </summary>
        public string SystemLanguage;

        /// <summary>
        /// 计算机上的所有逻辑驱动器列表
        /// </summary>
        public string Drives;

        /// <summary>
        /// 系统盘
        /// </summary>
        public string SystemDrive;

        /// <summary>
        /// 系统盘剩余空间，单位字节
        /// </summary>
        public long SystemDriveAvailableSize;

        /// <summary>
        /// 系统盘容量，单位字节
        /// </summary>
        public long SystemDriveTotalSize;

        /// <summary>
        /// 游戏安装目录
        /// </summary>
        public string InstallFolder;

        /// <summary>
        /// 游戏安装分区剩余空间，单位字节
        /// </summary>
        public long InstallDriveAvailableSize;

        /// <summary>
        /// 游戏安装分区容量，单位字节
        /// </summary>
        public long InstallDriveTotalSize;

        /// <summary>
        /// 设备屏幕分辨率，格式"1920 x 1080 @ 60Hz"
        /// </summary>
        public string DeviceResolution;

        /// <summary>
        /// 当前游戏分辨率
        /// </summary>
        public Vector2 GameResolution;
    }

    /// <summary>
    /// 提供获取环境软硬件信息的接口
    /// </summary>
    public static class DeviceUtils
    {
        private static Func<string> _userNameProvider;
        private static HardwareInfo _hardwareInfo;
        private static SoftwareInfo _softwareInfo;
        private static string _sessionId;

        /// <summary>
        /// 获取硬件环境信息
        /// </summary>
        /// <returns></returns>
        public static HardwareInfo GetHardwareInfo()
        {
            if (_hardwareInfo == null)
            {
                _hardwareInfo = new HardwareInfo
                {
                    DeviceModel = SystemInfo.deviceModel,
                    DeviceName = SystemInfo.deviceName,
                    UniqueDeviceId = SystemInfo.deviceUniqueIdentifier,
                    CpuModel = SystemInfo.processorType,
                    CpuCores = SystemInfo.processorCount,
                    CpuFrequency = SystemInfo.processorFrequency,
                    MemorySize = SystemInfo.systemMemorySize,
                    GraphicCard = SystemInfo.graphicsDeviceName,
                    VideoMemorySize = SystemInfo.graphicsMemorySize
                };
            }

            return _hardwareInfo;
        }

        /// <summary>
        /// 获取软件环境信息
        /// </summary>
        /// <returns></returns>
        public static SoftwareInfo GetSoftwareInfo()
        {
            if (_softwareInfo == null)
            {
                var sysDrive = Path.GetPathRoot(Environment.SystemDirectory);
                var installPath = Environment.CurrentDirectory;
                _softwareInfo = new SoftwareInfo
                {
                    Platform = Application.platform,
                    NetworkConnected = Application.internetReachability != NetworkReachability.NotReachable,
                    OsName = SystemInfo.operatingSystem,
                    OsVersion = Environment.OSVersion.Version,
                    SystemLanguage = CultureInfo.InstalledUICulture.Name,
                    SystemDrive = sysDrive,
                    InstallFolder = installPath,
                    DeviceResolution = Screen.currentResolution.ToString(),
                    GameResolution = new Vector2(Screen.width, Screen.height)
                };
                // var drives = DriveInfo.GetDrives();
                // var drivesStr = new StringBuilder();
                // foreach (var drive in drives)
                // {
                //     drivesStr.Append(drive.Name);
                //     drivesStr.Append('(');
                //     drivesStr.Append(drive.DriveType);
                //     drivesStr.Append(')');
                //     drivesStr.Append(',');
                //     if (drive.Name.Equals(sysDrive))
                //     {
                //         _softwareInfo.SystemDriveTotalSize = drive.TotalSize;
                //         _softwareInfo.SystemDriveAvailableSize = drive.AvailableFreeSpace;
                //     }
                //     if (drive.Name.Equals(Path.GetPathRoot(installPath)))
                //     {
                //         _softwareInfo.InstallDriveTotalSize = drive.TotalSize;
                //         _softwareInfo.InstallDriveAvailableSize = drive.AvailableFreeSpace;
                //     }
                // }
                // _softwareInfo.Drives = drivesStr.ToString(0, drivesStr.Length - 1);
                var drives = Directory.GetLogicalDrives();
                _softwareInfo.Drives = string.Join(",", drives);
                _softwareInfo.SystemDriveTotalSize = 0;
                _softwareInfo.SystemDriveAvailableSize = 0;
                _softwareInfo.InstallDriveTotalSize = 0;
                _softwareInfo.InstallDriveAvailableSize = 0;
            }

            return _softwareInfo;
        }

        /// <summary>
        /// 获取SessionId，每次启动游戏期间唯一，重启后重置
        /// </summary>
        /// <returns></returns>
        public static string GetSessionId()
        {
            if (_sessionId == null)
            {
                _sessionId = Guid.NewGuid().ToString();
            }

            return _sessionId;
        }

        /// <summary>
        /// 设置获取用户ID的自定义方法
        /// </summary>
        /// <param name="provider"></param>
        public static void SetUserNameProvider(Func<string> provider)
        {
            _userNameProvider = provider;
        }

        /// <summary>
        /// 获取可读的用户名，优先使用自定义的获取方法，否则返回计算机名
        /// </summary>
        /// <returns></returns>
        public static string GetUserName()
        {
            if (_userNameProvider != null)
            {
                var userId = "";
                if (string.IsNullOrEmpty(userId))
                {
                    return userId;
                }
            }

            return GetHardwareInfo().DeviceName;
        }
    }
}
