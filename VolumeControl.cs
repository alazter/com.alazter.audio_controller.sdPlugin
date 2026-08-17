using System;
using System.Runtime.InteropServices;

public class VolumeControl
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, System.Text.StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    private static string currentCorrelationId = "";
    private static System.Collections.Generic.Dictionary<uint, string> pidCache = new System.Collections.Generic.Dictionary<uint, string>();
    private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ISimpleAudioVolume>> targetCache = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<ISimpleAudioVolume>>();

    private static string GetProcNameFromPid(uint pid)
    {
        if (pid == 0) return null;
        string name;
        if (pidCache.TryGetValue(pid, out name)) return name;
        try
        {
            name = System.Diagnostics.Process.GetProcessById((int)pid).ProcessName.ToLower();
            pidCache[pid] = name;
            return name;
        }
        catch
        {
            pidCache[pid] = null;
            return null;
        }
    }

    private static void SendOutput(object value)
    {
        string prefix = string.IsNullOrEmpty(currentCorrelationId) ? "" : currentCorrelationId + "|";
        Console.WriteLine(prefix + (value != null ? value.ToString() : ""));
    }

    [ComImport, Guid("bcde0395-e52f-467c-8e3d-c4579291692e")]
    private class MMDeviceEnumerator { }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceCollection
    {
        [PreserveSig] int GetCount(out int pcDevices);
        [PreserveSig] int Item(int nDevice, out IMMDevice ppDevice);
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection ppDevices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionManager2
    {
        [PreserveSig] int GetAudioSessionControl();
        [PreserveSig] int GetSimpleAudioVolume();
        [PreserveSig] int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionEnumerator
    {
        [PreserveSig] int GetCount(out int SessionCount);
        [PreserveSig] int GetSession(int SessionCount, out IAudioSessionControl2 Session);
    }

    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioSessionControl2
    {
        [PreserveSig] int GetState(out int pRetVal);
        [PreserveSig] int GetDisplayName(out IntPtr pRetVal);
        [PreserveSig] int SetDisplayName(string Value, Guid EventContext);
        [PreserveSig] int GetIconPath(out IntPtr pRetVal);
        [PreserveSig] int SetIconPath(string Value, Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid pRetVal);
        [PreserveSig] int SetGroupingParam(Guid Override, Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr NewNotifications);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr NewNotifications);
        [PreserveSig] int GetSessionIdentifier(out IntPtr pRetVal);
        [PreserveSig] int GetSessionInstanceIdentifier(out IntPtr pRetVal);
        [PreserveSig] int GetProcessId(out uint pRetVal);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ISimpleAudioVolume
    {
        [PreserveSig] int SetMasterVolume(float fLevel, ref Guid EventContext);
        [PreserveSig] int GetMasterVolume(out float pfLevel);
        [PreserveSig] int SetMute(int bMute, ref Guid EventContext);
        [PreserveSig] int GetMute(out int pbMute);
    }

    private static string GetProcessPathSafe(System.Diagnostics.Process proc)
    {
        try
        {
            return proc.MainModule.FileName;
        }
        catch
        {
            // Fallback usando QueryFullProcessImageName para quando MainModule falhar com Acesso Negado (UWP/Admin)
            IntPtr hProcess = OpenProcess(0x1000, false, proc.Id); // PROCESS_QUERY_LIMITED_INFORMATION
            if (hProcess != IntPtr.Zero)
            {
                try
                {
                    int size = 1024;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(size);
                    if (QueryFullProcessImageName(hProcess, 0, sb, ref size))
                    {
                        return sb.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hProcess);
                }
            }
        }
        return null;
    }

    private static string GetProcessPathByName(string processName)
    {
        if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            processName = processName.Substring(0, processName.Length - 4);
        }
        try
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(processName);
            if (processes != null && processes.Length > 0)
            {
                foreach (var proc in processes)
                {
                    try
                    {
                        string path = GetProcessPathSafe(proc);
                        if (!string.IsNullOrEmpty(path)) return path;
                    }
                    catch {}
                }
            }
        }
        catch {}
        return null;
    }

    private static string[] ParseCommandLine(string line)
    {
        line = line.Trim();
        if (line.StartsWith("icon ", StringComparison.OrdinalIgnoreCase))
        {
            return new string[] { "icon", line.Substring(5).Trim() };
        }
        if (line.StartsWith("path ", StringComparison.OrdinalIgnoreCase))
        {
            return new string[] { "path", line.Substring(5).Trim() };
        }
        
        int lastSpace = line.LastIndexOf(' ');
        if (lastSpace == -1)
        {
            return new string[] { line };
        }
        
        string target = line.Substring(0, lastSpace).Trim();
        string actionOrVol = line.Substring(lastSpace + 1).Trim();
        return new string[] { target, actionOrVol };
    }

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("USAGE: VolumeControl.exe <pid_or_name|list|server> [new_volume_0_to_100|toggle_mute]");
            return;
        }

        string cmd = args[0].ToLower();
        if (cmd == "server" || cmd == "ipc")
        {
            string line;
            while ((line = Console.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Equals("quit", StringComparison.OrdinalIgnoreCase) || line.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                if (string.IsNullOrEmpty(line)) continue;
                
                string correlationId = "";
                string actualCommand = line;
                int pipeIndex = line.IndexOf('|');
                if (pipeIndex != -1)
                {
                    correlationId = line.Substring(0, pipeIndex).Trim();
                    actualCommand = line.Substring(pipeIndex + 1).Trim();
                }

                currentCorrelationId = correlationId;
                
                try
                {
                    string[] parts = ParseCommandLine(actualCommand);
                    ExecuteCommand(parts);
                }
                catch
                {
                    SendOutput("-1");
                }
            }
            return;
        }

        ExecuteCommand(args);
    }

    private static void ExecuteCommand(string[] args)
    {
        if (args.Length == 0) { SendOutput("-1"); return; }

        string cmd = args[0].ToLower();
        if (cmd == "path")
        {
            if (args.Length < 2) { SendOutput(""); return; }
            string targetName = args[1];
            string path = GetProcessPathByName(targetName);
            SendOutput(path ?? "");
            return;
        }

        if (cmd == "icon")
        {
            if (args.Length < 2) { SendOutput(""); return; }
            string target = args[1];
            string exePath = target;
            if (!System.IO.File.Exists(target))
            {
                string resolved = GetProcessPathByName(target);
                if (!string.IsNullOrEmpty(resolved) && System.IO.File.Exists(resolved))
                {
                    exePath = resolved;
                }
                else
                {
                    // Fallbacks comuns
                    if (target.Equals("steam.exe", StringComparison.OrdinalIgnoreCase) || target.Equals("steam", StringComparison.OrdinalIgnoreCase))
                    {
                        exePath = @"C:\Program Files (x86)\Steam\steam.exe";
                    }
                }
            }

            if (System.IO.File.Exists(exePath))
            {
                try
                {
                    using (System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath))
                    {
                        if (icon != null)
                        {
                            using (System.Drawing.Bitmap bmp = icon.ToBitmap())
                            {
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                {
                                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    byte[] bytes = ms.ToArray();
                                    SendOutput("data:image/png;base64," + Convert.ToBase64String(bytes));
                                    return;
                                }
                            }
                        }
                    }
                }
                catch {}
            }
            SendOutput("");
            return;
        }

        string targetInput = args[0].ToLower().Replace(".exe", "");
        bool listMode = (targetInput == "list");
        uint targetPid = 0;
        bool isPid = !listMode && uint.TryParse(targetInput, out targetPid);

        string targetProcessName = null;
        if (isPid)
        {
            try {
                targetProcessName = System.Diagnostics.Process.GetProcessById((int)targetPid).ProcessName.ToLower();
            } catch { }
        }
        else if (!listMode)
        {
            targetProcessName = targetInput;
        }

        if (listMode)
        {
            targetCache.Clear();
        }
        else if (!string.IsNullOrEmpty(targetInput) && targetCache.ContainsKey(targetInput))
        {
            var cachedList = targetCache[targetInput];
            if (cachedList != null && cachedList.Count > 0)
            {
                float lastVol = -1f;
                int lastMute = 0;
                try
                {
                    foreach (var simpleVol in cachedList)
                    {
                        if (args.Length > 1)
                        {
                            if (args[1].ToLower() == "toggle_mute")
                            {
                                int isMuted;
                                simpleVol.GetMute(out isMuted);
                                Guid ctx = Guid.Empty;
                                simpleVol.SetMute(isMuted == 0 ? 1 : 0, ref ctx);
                            }
                            else
                            {
                                float newVol;
                                if (float.TryParse(args[1], out newVol))
                                {
                                    newVol = Math.Max(0, Math.Min(1, newVol / 100f));
                                    Guid ctx = Guid.Empty;
                                    simpleVol.SetMasterVolume(newVol, ref ctx);
                                }
                            }
                        }
                        float curVol;
                        simpleVol.GetMasterVolume(out curVol);
                        int finalMute;
                        simpleVol.GetMute(out finalMute);
                        lastVol = curVol;
                        lastMute = finalMute;
                    }
                    if (lastVol >= 0)
                    {
                        SendOutput(Math.Round(lastVol * 100) + "|" + lastMute);
                        return;
                    }
                }
                catch
                {
                    targetCache.Remove(targetInput);
                }
            }
        }

        IMMDeviceEnumerator enumerator = null;
        IMMDeviceCollection deviceCol = null;

        try
        {
            enumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            if (enumerator.EnumAudioEndpoints(0, 1, out deviceCol) != 0 || deviceCol == null)
            {
                SendOutput("-1");
                return;
            }

            int deviceCount = 0;
            if (deviceCol.GetCount(out deviceCount) != 0)
            {
                SendOutput("-1");
                return;
            }

            if (listMode)
            {
                System.Collections.Generic.List<string> procNames = new System.Collections.Generic.List<string>();
                for (int d = 0; d < deviceCount; d++)
                {
                    IMMDevice device = null;
                    if (deviceCol.Item(d, out device) != 0 || device == null) continue;

                    try
                    {
                        Guid iid = typeof(IAudioSessionManager2).GUID;
                        object o;
                        if (device.Activate(ref iid, 1, IntPtr.Zero, out o) == 0 && o != null)
                        {
                            IAudioSessionManager2 manager = (IAudioSessionManager2)o;
                            IAudioSessionEnumerator sessionEnum;
                            if (manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null)
                            {
                                int count;
                                if (sessionEnum.GetCount(out count) == 0)
                                {
                                    for (int i = 0; i < count; i++)
                                    {
                                        IAudioSessionControl2 sessionCtrl;
                                        if (sessionEnum.GetSession(i, out sessionCtrl) == 0 && sessionCtrl != null)
                                        {
                                            try
                                            {
                                                uint pid;
                                                sessionCtrl.GetProcessId(out pid);
                                                if (pid == 0)
                                                {
                                                    IntPtr instIdPtr;
                                                    sessionCtrl.GetSessionInstanceIdentifier(out instIdPtr);
                                                    if (instIdPtr != IntPtr.Zero)
                                                    {
                                                        string instId = Marshal.PtrToStringUni(instIdPtr).ToLower();
                                                        Marshal.FreeCoTaskMem(instIdPtr);
                                                        if (instId.Contains(".exe%"))
                                                        {
                                                            int exeIdx = instId.LastIndexOf(".exe%");
                                                            int slashIdx = instId.LastIndexOf('\\', exeIdx);
                                                            if (slashIdx != -1 && exeIdx > slashIdx)
                                                            {
                                                                string extracted = instId.Substring(slashIdx + 1, exeIdx - slashIdx + 3);
                                                                if (!procNames.Contains(extracted)) procNames.Add(extracted);
                                                            }
                                                        }
                                                        else if (instId.Contains("spotify"))
                                                        {
                                                            if (!procNames.Contains("spotify.exe")) procNames.Add("spotify.exe");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        string processName = GetProcNameFromPid(pid);
                                                        if (!string.IsNullOrEmpty(processName))
                                                        {
                                                            string exeName = processName.ToLower() + ".exe";
                                                            if (!procNames.Contains(exeName)) procNames.Add(exeName);
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }
                                            finally
                                            {
                                                Marshal.ReleaseComObject(sessionCtrl);
                                            }
                                        }
                                    }
                                }
                                Marshal.ReleaseComObject(sessionEnum);
                            }
                            Marshal.ReleaseComObject(manager);
                        }
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(device);
                    }
                }
                SendOutput(string.Join(",", procNames));
                return;
            }

            bool foundAny = false;
            float lastVol = -1f;
            int lastMute = 0;

            for (int d = 0; d < deviceCount; d++)
            {
                IMMDevice device = null;
                if (deviceCol.Item(d, out device) != 0 || device == null) continue;

                try
                {
                    Guid iid = typeof(IAudioSessionManager2).GUID;
                    object o;
                    if (device.Activate(ref iid, 1, IntPtr.Zero, out o) == 0 && o != null)
                    {
                        IAudioSessionManager2 manager = (IAudioSessionManager2)o;
                        IAudioSessionEnumerator sessionEnum;
                        if (manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null)
                        {
                            int count;
                            if (sessionEnum.GetCount(out count) == 0)
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    IAudioSessionControl2 sessionCtrl;
                                    if (sessionEnum.GetSession(i, out sessionCtrl) == 0 && sessionCtrl != null)
                                    {
                                        try
                                        {
                                            uint pid;
                                            sessionCtrl.GetProcessId(out pid);

                                            bool match = false;
                                            IntPtr instIdPtr;
                                            sessionCtrl.GetSessionInstanceIdentifier(out instIdPtr);
                                            if (instIdPtr != IntPtr.Zero)
                                            {
                                                string instId = Marshal.PtrToStringUni(instIdPtr).ToLower();
                                                Marshal.FreeCoTaskMem(instIdPtr);
                                                if (!string.IsNullOrEmpty(targetProcessName))
                                                {
                                                    string nameWithoutExe = targetProcessName.Replace(".exe", "");
                                                    if (instId.Contains(targetProcessName) || instId.Contains(nameWithoutExe))
                                                    {
                                                        match = true;
                                                    }
                                                }
                                            }

                                            if (!match && pid != 0)
                                            {
                                                if (isPid && pid == targetPid)
                                                {
                                                    match = true;
                                                }
                                                else if (!string.IsNullOrEmpty(targetProcessName))
                                                {
                                                    try
                                                    {
                                                        string sName = GetProcNameFromPid(pid);
                                                        if (!string.IsNullOrEmpty(sName) && (sName == targetProcessName || sName + ".exe" == targetProcessName))
                                                        {
                                                            match = true;
                                                        }
                                                    }
                                                    catch { }
                                                }
                                            }

                                            if (match)
                                            {
                                                ISimpleAudioVolume simpleVol = sessionCtrl as ISimpleAudioVolume;
                                                if (simpleVol != null)
                                                {
                                                    if (!string.IsNullOrEmpty(targetInput))
                                                    {
                                                        if (!targetCache.ContainsKey(targetInput))
                                                        {
                                                            targetCache[targetInput] = new System.Collections.Generic.List<ISimpleAudioVolume>();
                                                        }
                                                        if (!targetCache[targetInput].Contains(simpleVol))
                                                        {
                                                            targetCache[targetInput].Add(simpleVol);
                                                        }
                                                    }
                                                    if (args.Length > 1)
                                                    {
                                                        if (args[1].ToLower() == "toggle_mute")
                                                        {
                                                            int isMuted;
                                                            simpleVol.GetMute(out isMuted);
                                                            Guid ctx = Guid.Empty;
                                                            simpleVol.SetMute(isMuted == 0 ? 1 : 0, ref ctx);
                                                        }
                                                        else
                                                        {
                                                            float newVol;
                                                            if (float.TryParse(args[1], out newVol))
                                                            {
                                                                newVol = Math.Max(0, Math.Min(1, newVol / 100f));
                                                                Guid ctx = Guid.Empty;
                                                                simpleVol.SetMasterVolume(newVol, ref ctx);
                                                            }
                                                        }
                                                    }
                                                    float curVol;
                                                    simpleVol.GetMasterVolume(out curVol);
                                                    int finalMute;
                                                    simpleVol.GetMute(out finalMute);
                                                    lastVol = curVol;
                                                    lastMute = finalMute;
                                                    foundAny = true;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            Marshal.ReleaseComObject(sessionCtrl);
                                        }
                                    }
                                }
                            }
                            Marshal.ReleaseComObject(sessionEnum);
                        }
                        Marshal.ReleaseComObject(manager);
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(device);
                }
            }

            if (foundAny)
            {
                SendOutput(Math.Round(lastVol * 100) + "|" + lastMute);
                return;
            }

            SendOutput("-1");
        }
        catch
        {
            SendOutput("-1");
        }
        finally
        {
            if (deviceCol != null) Marshal.ReleaseComObject(deviceCol);
            if (enumerator != null) Marshal.ReleaseComObject(enumerator);
        }
    }
}
