using Tera.Game;

namespace DamageMeter.UI
{
    public class AbnormalityVM : TSPropertyChanged
    {
        private int _abnormalityId;
        private int _stacks;
        private bool _isCategory;
        private HotDot.Types _category;

        public int AbnormalityId
        {
            get => _abnormalityId;
            set
            {
                if (_abnormalityId == value) return;
                _abnormalityId = value;
                NotifyPropertyChanged();
            }
        }

        public int Stacks
        {
            get => _stacks;
            set
            {
                if (_stacks == value) return;
                _stacks = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsCategory
        {
            get => _isCategory;
            set
            {
                if (_isCategory == value) return;
                _isCategory = value;
                NotifyPropertyChanged();
            }
        }

        public HotDot.Types Category
        {
            get => _category;
            set
            {
                if (_category == value) return;
                _category = value;
                NotifyPropertyChanged();
            }
        }

        public AbnormalityVM(int id, int stacks)
        {
            IsCategory = false;
            _abnormalityId = id;
            _stacks = stacks;
        }

        public AbnormalityVM(HotDot.Types category)
        {
            IsCategory = true;
            Category = category;
        }
    }
}