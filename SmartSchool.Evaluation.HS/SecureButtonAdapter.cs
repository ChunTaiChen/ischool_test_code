using SmartSchool.AccessControl;
using SmartSchool.Customization.PlugIn;

namespace SmartSchool.Evaluation
{
    class SecureButtonAdapter : ButtonAdapter, IFeature
    {
        public SecureButtonAdapter(string featureCode)
        {
            _feature_code = featureCode;
        }

        #region IFeature 成員

        private string _feature_code;
        public string FeatureCode
        {
            get { return _feature_code; }
        }

        #endregion
    }
}
