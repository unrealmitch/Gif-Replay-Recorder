using System;
using System.IO;
using GetSocialSdk.Capture.Scripts.Internal.Gif;
using GetSocialSdk.Capture.Scripts.Internal.Recorder;
using GetSocialSdk.Scripts.Internal.Util;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace GetSocialSdk.Capture.Scripts
{

    public class GetSocialCapture : MonoBehaviour
    {

        /// <summary>
        /// Defines how frames are captured.
        /// </summary>
        public enum GetSocialCaptureMode
        {
            /// <summary>
            /// Frames captured continuously with the give frame rate.
            /// </summary>
            Continuous = 0,

            /// <summary>
            /// CaptureFrame() has to be called to make a capture.
            /// </summary>
            Manual
        }

        #region Public fields

        /// <summary>
        /// Number of captured frames per second. Default is 15.
        /// </summary>
        public int captureFrameRate = 15;

        /// <summary>
        /// Capture mode.
        /// </summary>
        public GetSocialCaptureMode captureMode = GetSocialCaptureMode.Continuous;

        /// <summary>
        /// Max. number of captured frames during the session. Default is 50.
        /// </summary>
        public int maxCapturedFrames = 50;

        /// <summary>
        /// Number of displayed frames per second. Default is 30.
        /// </summary>
        public int playbackFrameRate = 30;

        /// <summary>
        /// Generated gif loops or played only once.
        /// </summary>
        public bool loopPlayback = true;

        /// <summary>
        /// Captured content.
        /// </summary>
        public Camera capturedCamera;

        public bool isSaving { get; protected set; } = false;
        public bool isRecorderer { get; protected set; } = false;
        public bool isSaved { get; protected set; } = false;
        public string resultFilePath {get; protected set;}

        #endregion

        #region Private variables

        protected string _captureId;
        protected float _elapsedTime;
        protected Recorder _recorder;
        protected const string GeneratedContentFolderName = "gifresult";
        protected static int totalRecords = 0;

        #endregion

        #region Public methods

        public virtual void StartCapture()
        {
            if (isSaving)
            {
                throw new Exception("[GifRecorder] Actually saving last gif. Impossible start still finish.");
            }

            InitSession();
            isRecorderer = false;
            isSaved = false;
            _recorder.CurrentState = Recorder.RecordingState.Recording;
        }

        public virtual bool StopCapture()
        {
            if (_recorder.CurrentState == Recorder.RecordingState.OnHold)
                return false;

            isRecorderer = true;
            _recorder.CurrentState = Recorder.RecordingState.OnHold;
            return true;
        }

        public virtual void ResumeCapture()
        {
            if (_captureId == null)
            {
                Debug.Log("There is no previous capture session to continue");
            }
            else
            {
                _recorder.CurrentState = Recorder.RecordingState.Recording;
            }
        }

        public virtual void CaptureFrame()
        {
            if (_captureId == null)
            {
                InitSession();
            }
            _recorder.CurrentState = Recorder.RecordingState.RecordNow;
        }

        public virtual void GenerateCapture(Action<byte[]> result)
        {
            isSaving = true;
            _recorder.CurrentState = Recorder.RecordingState.OnHold;
            if (StoreWorker.Instance.StoredFrames.Count() > 0)
            {
                var generator = new GeneratorWorker(loopPlayback, playbackFrameRate, ThreadPriority.BelowNormal, StoreWorker.Instance.StoredFrames,
                     resultFilePath,
                    () =>
                    {
                        Debug.Log("Result: " + resultFilePath);
                        MainThreadExecutor.Queue(() =>
                        {
                            isSaving = false;
                            isSaved = true;
                            result(File.ReadAllBytes(resultFilePath));
                        });
                    });
                generator.Start();
            }
            else
            {
                Debug.Log("Something went wrong, check your settings");
                result(new byte[0]);
            }
        }

        public virtual bool IsVideoRecordererCheckDeep()
        {
            return StoreWorker.Instance != null && StoreWorker.Instance.StoredFrames != null && StoreWorker.Instance.StoredFrames.Count() > 0;
        }

        public virtual void CleanAll(bool alsoCleanFiles){
            if(isSaving)
               throw new Exception("[GifReplay] Clean all while gif is saving is not possible!");

            StopCapture();
            isRecorderer = false;
            isSaved = false;
            StoreWorker.Instance.Clear();

            if(alsoCleanFiles){
                CleanUp();
            }
        }

        #endregion

        #region Unity methods

        protected virtual void Awake()
        {
            if (capturedCamera == null)
            {
                capturedCamera = GetComponent<Camera>();

                if (capturedCamera == null)
                    capturedCamera = Camera.main;
            }

            if (capturedCamera == null)
            {
                Debug.LogError("Camera is not set");
                return;
            }

            _recorder = capturedCamera.GetComponent<Recorder>();

            if (_recorder == null)
            {
                _recorder = capturedCamera.gameObject.AddComponent<Recorder>();
            }

            _recorder.CaptureFrameRate = captureFrameRate;
        }

        protected virtual void OnDestroy()
        {
            StoreWorker.Instance.Clear();
        }

        #endregion

        #region Private methods

        protected static string GetResultDirectory()
        {
            string resultDirPath;
#if UNITY_EDITOR
            resultDirPath = Application.dataPath;
#else
            resultDirPath = Application.persistentDataPath; 
#endif
            resultDirPath += Path.DirectorySeparatorChar + GeneratedContentFolderName;

            if (!Directory.Exists(resultDirPath))
            {
                Directory.CreateDirectory(resultDirPath);
            }
            return resultDirPath;
        }

        protected virtual void InitSession()
        {
            _captureId = DateTimeNow + "_" + totalRecords++;
            var fileName = string.Format("GifRecorder_{0}.gif", _captureId);
            resultFilePath = GetResultDirectory() + Path.DirectorySeparatorChar + fileName;
            StoreWorker.Instance.Start(ThreadPriority.BelowNormal, maxCapturedFrames);
        }

        protected virtual void CleanUp()
        {
            if (isSaving)
            {
                throw new Exception("[GifRecorder] Actually saving last gif. Impossible CleanUp still finish.");
            }

            if (File.Exists(resultFilePath))
            {
                File.Delete(resultFilePath);
            }

            resultFilePath = "";
        }

        protected virtual string DateTimeNow => DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");

        #endregion

    }

}