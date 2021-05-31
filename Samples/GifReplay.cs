using GetSocialSdk.Capture.Scripts;
using UnityEngine;

public class GifReplay : MonoBehaviour
{
    public GetSocialCapture capture;
    public GetSocialCapturePreview capturePreview;

    public float durationReplay = 5f;

    // Start is called before the first frame update
    void Start()
    {
        capture.maxCapturedFrames = (int) (capture.captureFrameRate * durationReplay);
        StartContinuous();
    }

    [ContextMenu("Stop&Replay")]
    public void StopAndReplay()
    {
        StopAndSave();
        capturePreview.Play();
    }

    public void StopAndSave()
    {
        capture.StopCapture();
        // generate gif
        capture.GenerateCapture(result =>
        {
            Debug.Log("Generated gif of: " + (result.Length/1024) + "kB");
        });
    }

    public void StartContinuous(){
        capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Continuous;
        capture.StartCapture();
    }
    public void StartManual()
    {
        capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Manual;
        capture.StartCapture();
    }

    public void RecordFrame()
    {
        capture.CaptureFrame();
    }
}
