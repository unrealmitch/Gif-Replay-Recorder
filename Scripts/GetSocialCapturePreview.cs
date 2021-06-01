using System;
using System.Collections.Generic;
using GetSocialSdk.Capture.Scripts.Internal.Recorder;
using UnityEngine;
using UnityEngine.UI;

namespace GetSocialSdk.Capture.Scripts
{
    public class GetSocialCapturePreview : MonoBehaviour
    {

        #region Public fields

        /// <summary>
        /// Number of displayed frames per second. Default is 30.
        /// </summary>
        public int playbackFrameRate = 30;

        /// <summary>
        /// Preview loops or played only once.
        /// </summary>
        public bool loopPlayback = true;

        public RawImage _rawImage;

        #endregion

        #region Private fields

        protected List<Texture2D> _framesToPlay;

        protected bool _play;
        protected float _playbackStartTime;
        protected bool _previewInitialized;

        #endregion

        #region Public methods

        /// <summary>
        /// Starts preview playback.
        /// </summary>
        public virtual void Play(bool forceReinit = true)
        {
            if (!_previewInitialized || forceReinit)
            {
                Init();
            }
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            _play = true;
        }

        /// <summary>
        /// Stops playback.
        /// </summary>
        public virtual void Stop()
        {
            _play = false;
        }

        /// <summary>
        /// Stops playback and clear frames to reduce memory consumption
        /// </summary>
        public virtual void StopAndClearFrames()
        {
            Stop();
            if(_rawImage)
                _rawImage.texture = null;
            ClearFrames();
            _previewInitialized = false;
        }

        #endregion

        #region Private methods

        protected virtual void Init()
        {
            ClearFrames();

            if (_framesToPlay == null)
                _framesToPlay = new List<Texture2D>();

            for (var i = 0; i < StoreWorker.Instance.StoredFrames.Count(); i++)
            {
                var frame = StoreWorker.Instance.StoredFrames.ElementAt(i);
                var texture2D = new Texture2D(frame.Width, frame.Height);
                texture2D.SetPixels32(frame.Data);
                texture2D.Apply();
                _framesToPlay.Add(texture2D);
            }

            _previewInitialized = true;

            if (_framesToPlay.Count == 0)
            {
                _play = false;
            }
        }

        #endregion

        #region Unity methods

        protected virtual void Awake()
        {
            if (_rawImage == null)
                _rawImage = GetComponent<RawImage>();

            _framesToPlay = new List<Texture2D>();
            _play = false;
        }

        protected virtual void OnDestroy()
        {
            ClearFrames();
            _framesToPlay = null;
        }

        protected virtual void ClearFrames()
        {
            if (_framesToPlay != null && _framesToPlay.Count > 0)
            {
                foreach (Texture2D frame in _framesToPlay)
                {
                    if (frame != null)
                        Destroy(frame);
                }

                var listId = GC.GetGeneration(_framesToPlay);
                _framesToPlay.Clear();
                GC.Collect(listId, GCCollectionMode.Forced);
                _framesToPlay = new List<Texture2D>();
            }
        }

        protected virtual void Start()
        {
            Play(false);
        }

        protected virtual void Update()
        {
            if (!_play) return;
            if (_framesToPlay == null || _framesToPlay.Count == 0) return;
            if (Math.Abs(_playbackStartTime) < 0.0001f)
            {
                _playbackStartTime = Time.time;
            }
            var index = (int)((Time.time - _playbackStartTime) * playbackFrameRate) % _framesToPlay.Count;
            _rawImage.texture = _framesToPlay[index];
            if (index == _framesToPlay.Count - 1 && !loopPlayback)
            {
                _play = false;
            }
        }

        #endregion

    }
}