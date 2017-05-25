namespace CoverMaker {
    class ViewModel : BindableBase {
        private bool _renseiischecked = true;
        private bool _idle = true;
        private string _processstring;
        private double _progressvalue;

        public bool RenseiIsChecked { get => _renseiischecked; set => SetField(ref _renseiischecked, value); }
        public bool Idle { get => _idle; set => SetField(ref _idle, value); }
        public double ProgressValue { get => _progressvalue; set => SetField(ref _progressvalue, value); }
        public string ProcessString { get => _processstring; set => SetField(ref _processstring, value); }
    }
}
