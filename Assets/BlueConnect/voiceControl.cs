using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class voiceControl : MonoBehaviour
{
    //public string[] keywords = { "��ʼ", "���", "����", "ͣ��", "��", "˳", "����", "����", "��", "��", "up", "down" };
    //public PhraseRecognizer voiceDetect;
    //public ConfidenceLevel confidenceLevel = ConfidenceLevel.Medium;
    //public ControlTello controlTello;
    //private void Start()
    //{
    //    controlTello = GetComponent<ControlTello>();
    //    if(voiceDetect == null)
    //    {
    //        voiceDetect = new KeywordRecognizer(keywords, confidenceLevel);
    //        voiceDetect.OnPhraseRecognized += M_PhraseRecognizer_OnPhraseRecognized;
    //        voiceDetect.Start();
    //        print("start");
    //    }
    //}
    //public void M_PhraseRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    //{
    //    print(args.text);
    //    commands(args.text);
    //}
    //private void OnDestroy()
    //{
    //    if(voiceDetect != null)
    //    {
    //        voiceDetect.Dispose();
    //    }
    //}
    //public void commands(string texts)
    //{
    //    switch (texts)
    //    {
    //        case "��ʼ":
    //            controlTello.sendData("command");
    //            break;
    //        case "���":
    //            controlTello.sendData("takeoff");
    //            break;
    //        case "����":
    //            controlTello.sendData("land");
    //            break;
    //        case "ͣ��":
    //            controlTello.sendData("emergency");
    //            break;
    //        case "��":
    //            controlTello.sendData("ccw 90");
    //            break;
    //        case "˳":
    //            controlTello.sendData("cw 90");
    //            break;
    //        case "����":
    //            controlTello.sendData("forward 20");
    //            break;
    //        case "����":
    //            controlTello.sendData("back 20");
    //            break;
    //        case "��":
    //            controlTello.sendData("left 20");
    //            break;
    //        case "��":
    //            controlTello.sendData("right 20");
    //            break;
    //        case "up":
    //            controlTello.sendData("up 20");
    //            break;
    //        case "down":
    //            controlTello.sendData("down 20");
    //            break;
    //    }
    //}
}
