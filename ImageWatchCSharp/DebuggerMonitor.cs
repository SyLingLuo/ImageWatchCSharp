using EnvDTE;
using EnvDTE80;
using ImageWatchCSharp;
using ImageWatchCSharp.ViewModels;
using Microsoft.Diagnostics.Runtime;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class DebuggerMonitor
{
    private static DebuggerMonitor _instance;
    private static readonly object _lock = new object();

    private DTE2 _dte;
    private DebuggerEvents _debuggerEvents;

    public int ProcessID { get; private set; }

    public IntPtr ProcessHandle { get; private set; }
    public MainViewModel MainViewModel { get; set; }

    /// <summary>
    /// 获取唯一实例
    /// </summary>
    public static DebuggerMonitor Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DebuggerMonitor();
                    }
                }
            }
            return _instance;
        }
    }

    public DTE2 DTE => _dte;

    internal static Action<bool> MatViewerCommand_SetEnabled { get; set; }
    internal static Action CloseAllMatViewerWindows { get; set; }

    private void SetEnabled(bool enabled) => MatViewerCommand_SetEnabled?.Invoke(enabled);
    private void CloseAllInstances() => CloseAllMatViewerWindows?.Invoke();

    private DataTarget dataTarget;
    /// <summary>
    /// 初始化监听器（需在主线程调用）
    /// </summary>
    /// <param name="dte">DTE 对象</param>
    public void Initialize(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (dte == null)
            throw new ArgumentNullException(nameof(dte));

        if (_dte != null)
            throw new InvalidOperationException("DebuggerMonitor is Initialized。");

        _dte = dte;
        _debuggerEvents = _dte.Events.DebuggerEvents;

        _debuggerEvents.OnEnterRunMode += OnEnterRunMode;
        _debuggerEvents.OnEnterBreakMode += OnEnterBreakMode;
        _debuggerEvents.OnExceptionThrown += OnExceptionThrown;
        ProcessID = GetCurrentProcessId() ;
        ProcessHandle = GetProcessHandle(ProcessID);
        MainViewModel = new MainViewModel(_dte);

    }

    private void OnEnterRunMode(dbgEventReason reason)
    {
    }

    private void OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
    {
        if(ProcessID!= GetCurrentProcessId())
        {
            ProcessID = GetCurrentProcessId();
            ReleaseProcessHandle();
            ProcessHandle = GetProcessHandle(ProcessID);
        }
        MainViewModel?.RefreshImages();
    }

    private void OnEnterDesignMode(string Name, [In] int Code, [In][MarshalAs(UnmanagedType.BStr)] string Description, [In][Out] ref dbgExceptionAction ExceptionAction)
    {

    }

    private void OnExceptionThrown(string ExceptionType, [In][MarshalAs(UnmanagedType.BStr)] string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
    {
    }


    public bool IsInBreakMode =>
        _dte?.Debugger?.CurrentMode == dbgDebugMode.dbgBreakMode;

    public int GetCurrentProcessId()
    {
        try
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var process = _dte?.Debugger?.CurrentProcess;
            if (process == null)
                return 0;
            return process.ProcessID;
        }
        catch
        {
            return 0;
        }
    }

    private IntPtr GetProcessHandle(int processId)
    {
        if (processId <= 0)
            return IntPtr.Zero;

        IntPtr handle = WinAPI.OpenProcess(ProcessAccessFlags.PROCESS_VM_READ | ProcessAccessFlags.PROCESS_QUERY_INFORMATION,
            false, (uint)processId);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
        }
        
        return handle;
    }
    
    private void ReleaseProcessHandle()
    {
        if (ProcessHandle != IntPtr.Zero)
        {
            WinAPI.CloseHandle(ProcessHandle);
            ProcessHandle = IntPtr.Zero;
        }
    }
}