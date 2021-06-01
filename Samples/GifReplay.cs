using GetSocialSdk.Capture.Scripts;
using UnityEngine;
using UnityEngine.Events;

public class GifReplay : MonoBehaviour
{
    public GetSocialCapture capture;
    public GetSocialCapturePreview capturePreview;

    public float durationReplay = 5f;

    public UnityEvent onStartRecord, onStopRecord, onSaveRecord;

    // Start is called before the first frame update
    void Start()
    {
        capture.maxCapturedFrames = (int)(capture.captureFrameRate * durationReplay);
        StartContinuous();
    }

    public void StartContinuous()
    {
        capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Continuous;
        StartRecord();
    }
    public void StartManual()
    {
        capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Manual;
        StartRecord();
    }

    public void RecordFrame()
    {
        capture.CaptureFrame();
    }

    public void StopAndReplay()
    {
        StopRecord();
        capturePreview.Play();
    }

    public void StopRecord()
    {
        capture.StopCapture();
        onStopRecord?.Invoke();
    }

    public void StopAndSave()
    {
        StopRecord();
        capture.GenerateCapture(result =>
        {
            Debug.Log("Generated gif of: " + (result.Length / 1024) + "kB");
            onSaveRecord?.Invoke();
        });
    }

    private void StartRecord()
    {
        capture.StartCapture();
        onStartRecord?.Invoke();
    }
}
