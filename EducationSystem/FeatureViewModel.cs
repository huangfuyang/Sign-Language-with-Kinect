
namespace EducationSystem
{
    public class FeatureViewModel
    {
        public string FeatureName { get; set; }
        public string Value { get; set; }

        public FeatureViewModel(string featureName, string value)
        {
            this.FeatureName = featureName;
            this.Value = value;
        }
    }
}
