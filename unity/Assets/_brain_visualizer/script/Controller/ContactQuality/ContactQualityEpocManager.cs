using UnityEngine;
using UnityEngine.UI;
using Zenject;
using EmotivUnityPlugin;

namespace dirox.emotiv.controller{
    public class ContactQualityEpocManager : ContactQualityBaseManager
    {
        [SerializeField] Image CMS, DRL, F3, AF3, F7, FC5, F4, AF4, F8, FC6, T8, P8, O2, O1, P7, T7;

        [Inject(Id = "CMS")] ContactQualityNodeView CMSView;
        [Inject(Id = "DRL")] ContactQualityNodeView DRLView;
        [Inject(Id = "F3")]  ContactQualityNodeView F3View;
        [Inject(Id = "AF3")] ContactQualityNodeView AF3View;
        [Inject(Id = "F7")]  ContactQualityNodeView F7View;
        [Inject(Id = "FC5")] ContactQualityNodeView FC5View;
        [Inject(Id = "F4")]  ContactQualityNodeView F4View;
        [Inject(Id = "AF4")] ContactQualityNodeView AF4View;
        [Inject(Id = "F8")]  ContactQualityNodeView F8View;
        [Inject(Id = "FC6")] ContactQualityNodeView FC6View;
        [Inject(Id = "T8")]  ContactQualityNodeView T8View;
        [Inject(Id = "P8")]  ContactQualityNodeView P8View;
        [Inject(Id = "O2")]  ContactQualityNodeView O2View;
        [Inject(Id = "O1")]  ContactQualityNodeView O1View;
        [Inject(Id = "P7")]  ContactQualityNodeView P7View;
        [Inject(Id = "T7")]  ContactQualityNodeView T7View;

        protected override void SetColorSettingForNodes(Color[] allColors)
        {
            CMSView.SetColors(allColors).SetDisplay(CMS);
            DRLView.SetColors(allColors).SetDisplay(DRL);
            F3View.SetColors(allColors).SetDisplay(F3);
            AF3View.SetColors(allColors).SetDisplay(AF3);
            F7View.SetColors(allColors).SetDisplay(F7);
            FC5View.SetColors(allColors).SetDisplay(FC5);
            F4View.SetColors(allColors).SetDisplay(F4);
            AF4View.SetColors(allColors).SetDisplay(AF4);
            F8View.SetColors(allColors).SetDisplay(F8);
            FC6View.SetColors(allColors).SetDisplay(FC6);
            T8View.SetColors(allColors).SetDisplay(T8); 
            P8View.SetColors(allColors).SetDisplay(P8);
            O2View.SetColors(allColors).SetDisplay(O2);
            O1View.SetColors(allColors).SetDisplay(O1);
            P7View.SetColors(allColors).SetDisplay(P7);
            T7View.SetColors(allColors).SetDisplay(T7);
        }

        public override void SetContactQualityColor(ContactQualityValue[] contacts)
        {
            if (contacts == null) {
                CMSView.SetDisplay(CMS).SetQuality(ContactQualityValue.NO_SIGNAL);
                DRLView.SetDisplay(DRL).SetQuality(ContactQualityValue.NO_SIGNAL);
                F3View.SetDisplay(F3).SetQuality(ContactQualityValue.NO_SIGNAL);
                AF3View.SetDisplay(AF3).SetQuality(ContactQualityValue.NO_SIGNAL);
                F7View.SetDisplay(F7).SetQuality(ContactQualityValue.NO_SIGNAL);
                FC5View.SetDisplay(FC5).SetQuality(ContactQualityValue.NO_SIGNAL);
                F4View.SetDisplay(F4).SetQuality(ContactQualityValue.NO_SIGNAL);
                AF4View.SetDisplay(AF4).SetQuality(ContactQualityValue.NO_SIGNAL);
                F8View.SetDisplay(F8).SetQuality(ContactQualityValue.NO_SIGNAL);
                FC6View.SetDisplay(FC6).SetQuality(ContactQualityValue.NO_SIGNAL);
                T8View.SetDisplay(T8).SetQuality(ContactQualityValue.NO_SIGNAL);
                P8View.SetDisplay(P8).SetQuality(ContactQualityValue.NO_SIGNAL);
                O2View.SetDisplay(O2).SetQuality(ContactQualityValue.NO_SIGNAL);
                O1View.SetDisplay(O1).SetQuality(ContactQualityValue.NO_SIGNAL);
                P7View.SetDisplay(P7).SetQuality(ContactQualityValue.NO_SIGNAL);
                T7View.SetDisplay(T7).SetQuality(ContactQualityValue.NO_SIGNAL);

                return;
            }
                
            CMSView.SetDisplay(CMS).SetQuality(contacts[(int)Channels.CMS]);
            DRLView.SetDisplay(DRL).SetQuality(contacts[(int)Channels.DRL]);
            F3View.SetDisplay(F3).SetQuality(contacts[(int)Channels.F3]);
            AF3View.SetDisplay(AF3).SetQuality(contacts[(int)Channels.AF3]);
            F7View.SetDisplay(F7).SetQuality(contacts[(int)Channels.F7]);
            FC5View.SetDisplay(FC5).SetQuality(contacts[(int)Channels.FC5]);
            F4View.SetDisplay(F4).SetQuality(contacts[(int)Channels.F4]);
            AF4View.SetDisplay(AF4).SetQuality(contacts[(int)Channels.AF4]);
            F8View.SetDisplay(F8).SetQuality(contacts[(int)Channels.F8]);
            FC6View.SetDisplay(FC6).SetQuality(contacts[(int)Channels.FC6]);
            T8View.SetDisplay(T8).SetQuality(contacts[(int)Channels.T8]);
            P8View.SetDisplay(P8).SetQuality(contacts[(int)Channels.P8]);
            O2View.SetDisplay(O2).SetQuality(contacts[(int)Channels.O2]);
            O1View.SetDisplay(O1).SetQuality(contacts[(int)Channels.O1]);
            P7View.SetDisplay(P7).SetQuality(contacts[(int)Channels.P7]);
            T7View.SetDisplay(T7).SetQuality(contacts[(int)Channels.T7]);
        } 
    }
}


