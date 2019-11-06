using System;
using System.Threading;
using UnityEngine;
using Intel.RealSense;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
/// <summary>
/// Manages streaming using a RealSense Device
/// </summary>
[HelpURL("https://github.com/IntelRealSense/librealsense/tree/master/wrappers/unity")]
public class RsDevice : RsFrameProvider
{
    /// <summary>
    /// The parallelism mode of the module
    /// </summary>
    public enum ProcessMode
    {
        Multithread,
        UnityThread,
    }

    // public static RsDevice Instance { get; private set; }

    /// <summary>
    /// Threading mode of operation, Multithread or UnityThread
    /// </summary>
    [Tooltip("Threading mode of operation, Multithreads or Unitythread")]
    public ProcessMode processMode;

    // public bool Streaming { get; private set; }

    /// <summary>
    /// Notifies upon streaming start
    /// </summary>
    public override event Action<PipelineProfile> OnStart;

    /// <summary>
    /// Notifies when streaming has stopped
    /// </summary>
    public override event Action OnStop;

    /// <summary>
    /// Fired when a new frame is available
    /// </summary>
    public override event Action<Frame> OnNewSample;

    /// <summary>
    /// User configuration
    /// </summary>
    public RsConfiguration DeviceConfiguration = new RsConfiguration
    {
        mode = RsConfiguration.Mode.Live,
        RequestedSerialNumber = string.Empty,
        Profiles = new RsVideoStreamRequest[] {
            new RsVideoStreamRequest {Stream = Stream.Depth, StreamIndex = -1, Width = 640, Height = 480, Format = Format.Z16 , Framerate = 30 },
            new RsVideoStreamRequest {Stream = Stream.Infrared, StreamIndex = -1, Width = 640, Height = 480, Format = Format.Y8 , Framerate = 30 },
            new RsVideoStreamRequest {Stream = Stream.Color, StreamIndex = -1, Width = 640, Height = 480, Format = Format.Rgb8 , Framerate = 30 }
        }
    };

    private Thread worker;
    private readonly AutoResetEvent stopEvent = new AutoResetEvent(false);
    private Pipeline m_pipeline;



    void OnEnable()
    {
        m_pipeline = new Pipeline();

        using (var cfg = DeviceConfiguration.ToPipelineConfig())
            ActiveProfile = m_pipeline.Start(cfg);

        DeviceConfiguration.Profiles = ActiveProfile.Streams.Select(RsVideoStreamRequest.FromProfile).ToArray();

        if (processMode == ProcessMode.Multithread)
        {
            stopEvent.Reset();
            worker = new Thread(WaitForFrames);
            worker.IsBackground = true;
            worker.Start();
        }

        StartCoroutine(WaitAndStart());
    }

    IEnumerator WaitAndStart()
    {
        yield return new WaitForEndOfFrame();
        Streaming = true;
        if (OnStart != null)
            OnStart(ActiveProfile);
    }

    void OnDisable()
    {
        OnNewSample = null;
        // OnNewSampleSet = null;

        if (worker != null)
        {
            stopEvent.Set();
            worker.Join();
        }

        if (Streaming && OnStop != null)
            OnStop();

        if (ActiveProfile != null)
        {
            ActiveProfile.Dispose();
            ActiveProfile = null;
        }

        if (m_pipeline != null)
        {
            // if (Streaming)
            // m_pipeline.Stop();
            m_pipeline.Dispose();
            m_pipeline = null;
        }

        Streaming = false;
    }

    void OnDestroy()
    {
        // OnStart = null;
        OnStop = null;

        if (ActiveProfile != null)
        {
            ActiveProfile.Dispose();
            ActiveProfile = null;
        }

        if (m_pipeline != null)
        {
            m_pipeline.Dispose();
            m_pipeline = null;
        }
    }

    private void RaiseSampleEvent(Frame frame)
    {
        var onNewSample = OnNewSample;
        if (onNewSample != null)
        {
            onNewSample(frame);
        }
    }

    /// <summary>
    /// Worker Thread for multithreaded operations
    /// </summary>
    Align align = new Align(Stream.Color);
    CSRegister.CSRegisterTCD regClass = new CSRegister.CSRegisterTCD(
                    "C:\\Users\\aljuasin\\Desktop\\captura_tcd\\_imagenes\\patron\\output_imagesx640_80\\spaceing24_extrinsics_640_80.xml", true);
    FrameSet fs;
    int counter = 0;
    FrameQueue qColor;
    FrameQueue qDepth;
    //Frame test;
    //Predicate<Frame> matcher = new Predicate<Frame>();
    private void WaitForFrames()
    {
        while (!stopEvent.WaitOne(0))
        {
            using (var frames = m_pipeline.WaitForFrames())
            {
                using (Frame aligned_frames = align.Process(frames))
                {
                    if (aligned_frames.IsComposite)
                    {
                        using (FrameSet fs = aligned_frames.As<FrameSet>())
                        {
                            using (Frame color = fs.ColorFrame)
                            {
                                using (Frame depth = fs.DepthFrame)
                                {
                                    RaiseSampleEvent(aligned_frames);
                                    regClass.writeGRAYimage(depth.Data, "imageTest/depth_" + (counter) + ".png", 640, 480);
                                    regClass.writeRGBimage(color.Data, "imageTest/color_" + (counter++) + ".png", 640, 480);

                                }
                            }
                        }
                    }
                }

                //Frame aligned_frames = align.Process(frames);
                //test = aligned_frames.Clone();
                //regClass.writeRGBimage();
                //if (test.IsComposite)
                //{
                //    fs = test.As<FrameSet>(); 
                //    regClass.writeRGBimage(fs.ColorFrame.Data, "imageTest/col_" + (counter) + ".png", 640, 480);
                //    regClass.writeGRAYimage(fs.DepthFrame.Data, "imageTest/dep_" + (counter++) + ".png", 640, 480);
                //}
                //IntPtr a = frames.ColorFrame.Data;
                //regClass.writeRGBimage(frames.ColorFrame.Data, "imageTest/col_" + (counter) + ".png", 640, 480);
                //regClass.writeGRAYimage(aligned_frames.Data, "imageTest/dep_" + (counter++) + ".png", 640, 480);
                //fs.Dispose();
                //RaiseSampleEvent(frames);
                //RaiseSampleEvent(aligned_frames);
                //aligned_frames.Dispose();
                //Marshal.FreeHGlobal(a);
            }
        }
    }

    void Update()
    {
        if (!Streaming)
            return;

        if (processMode != ProcessMode.UnityThread)
            return;

        FrameSet frames;
        if (m_pipeline.PollForFrames(out frames))
        {
            using (frames)
                RaiseSampleEvent(frames);
        }
    }

}
