using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class voiceControl : MonoBehaviour
{
    public string[] keywords = { "¿ªÊ¼", "Æð·É", "½µÂä", "Í£»ú", "Äæ", "Ë³", "½ø¹¥", "³·ÍË", "×ó", "ÓÒ", "up", "down" };
    public PhraseRecognizer voiceDetect;
    public ConfidenceLevel confidenceLevel = ConfidenceLevel.Medium;
    public ControlTello controlTello;
    private void Start()
    {
        controlTello = GetComponent<ControlTello>();
        if(voiceDetect == null)
        {
            voiceDetect = new KeywordRecognizer(keywords, confidenceLevel);
            voiceDetect.OnPhraseRecognized += M_PhraseRecognizer_OnPhraseRecognized;
            voiceDetect.Start();
            print("start");
        }
    }
    public void M_PhraseRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        print(args.text);
        commands(args.text);
    }
    private void OnDestroy()
    {
        if(voiceDetect != null)
        {
            voiceDetect.Dispose();
        }
    }
    public void commands(string texts)
    {
        switch (texts)
        {
            case "¿ªÊ¼":
                controlTello.sendData("command");
                break;
            case "Æð·É":
                controlTello.sendData("takeoff");
                break;
            case "½µÂä":
                controlTello.sendData("land");
                break;
            case "Í£»ú":
                controlTello.sendData("emergency");
                break;
            case "Äæ":
                controlTello.sendData("ccw 90");
                break;
            case "Ë³":
                controlTello.sendData("cw 90");
                break;
            case "½ø¹¥":
                controlTello.sendData("forward 20");
                break;
            case "³·ÍË":
                controlTello.sendData("back 20");
                break;
            case "×ó":
                controlTello.sendData("left 20");
                break;
            case "ÓÒ":
                controlTello.sendData("right 20");
                break;
            case "up":
                controlTello.sendData("up 20");
                break;
            case "down":
                controlTello.sendData("down 20");
                break;
        }
    }
}
