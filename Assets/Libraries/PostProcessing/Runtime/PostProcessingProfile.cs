using System;

namespace UnityEngine.PostProcessing
{
    public class PostProcessingProfile : ScriptableObject
    {
        #pragma warning disable 0169 // "field x is never used"

        public BuiltinDebugViewsModel debugViews = new();
        public FogModel fog = new();
        public AntialiasingModel antialiasing = new();
        public AmbientOcclusionModel ambientOcclusion = new();
        public ScreenSpaceReflectionModel screenSpaceReflection = new();
        public DepthOfFieldModel depthOfField = new();
        public MotionBlurModel motionBlur = new();
        public EyeAdaptationModel eyeAdaptation = new();
        public BloomModel bloom = new();
        public ColorGradingModel colorGrading = new();
        public UserLutModel userLut = new();
        public ChromaticAberrationModel chromaticAberration = new();
        public GrainModel grain = new();
        public VignetteModel vignette = new();
        public DitheringModel dithering = new();

#if UNITY_EDITOR
        // Monitor settings
        [Serializable]
        public class MonitorSettings
        {
            // Callback used in the editor to grab the rendered frame and sent it to monitors
            public Action<RenderTexture> onFrameEndEditorOnly;

            // Global
            public int currentMonitorID = 0;
            public bool refreshOnPlay = false;

            // Histogram
            public enum HistogramMode
            {
                Red = 0,
                Green = 1,
                Blue = 2,
                Luminance = 3,
                RGBMerged,
                RGBSplit
            }

            public HistogramMode histogramMode = HistogramMode.Luminance;

            // Waveform
            public float waveformExposure = 0.12f;
            public bool waveformY = false;
            public bool waveformR = true;
            public bool waveformG = true;
            public bool waveformB = true;

            // Parade
            public float paradeExposure = 0.12f;

            // Vectorscope
            public float vectorscopeExposure = 0.12f;
            public bool vectorscopeShowBackground = true;
        }

        public MonitorSettings monitors = new();
#endif
    }
}
