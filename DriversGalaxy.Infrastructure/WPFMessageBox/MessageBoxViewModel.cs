using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace MessageBoxUtils
{
    internal class MessageBoxViewModel : INotifyPropertyChanged
    {
        public MessageBoxResult Result { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isOkDefault;
        private bool _isContinueDefault;
        private bool _isYesDefault;
        private bool _isNoDefault;
        private bool _isCancelDefault;

        private string _lang;
        private string _title;
        private string _message;
        private WPFMessageBoxButton _buttonOption;
        private MessageBoxOptions _options;

        private Visibility _yesNoVisibility;
        private Visibility _cancelVisibility;
        private Visibility _okVisibility;
        private Visibility _continueVisibility;

        private HorizontalAlignment _contentTextAlignment;
        private FlowDirection _contentFlowDirection;
        private FlowDirection _titleFlowDirection;

        private ICommand _yesCommand;
        private ICommand _continueCommand;
        private ICommand _noCommand;
        private ICommand _cancelCommand;
        private ICommand _okCommand;
        private ICommand _escapeCommand;
        private ICommand _closeCommand;

        private WPFMessageBoxWindow _view;
        private ImageSource _messageImageSource;

        public MessageBoxViewModel(
            WPFMessageBoxWindow view,
            CultureInfo culture,
            string title,
            string message,
            WPFMessageBoxButton buttonOption,
            MessageBoxImage image,
            MessageBoxResult defaultResult,
            MessageBoxOptions options)
        {
            //TextAlignment         
            Title = title;
            Message = message;
            ButtonOption = buttonOption;
            Options = options;
            SetDirections(options);
            SetButtonVisibility(buttonOption);
            SetImageSource(image);
            SetButtonDefault(defaultResult);
            _view = view;
        }

        public WPFMessageBoxButton ButtonOption
        {
            get { return _buttonOption; }
            set
            {
                if (_buttonOption != value)
                {
                    _buttonOption = value;
                    NotifyPropertyChange("ButtonOption");
                }
            }
        }

        public MessageBoxOptions Options
        {
            get { return _options; }
            set
            {
                if (_options != value)
                {
                    _options = value;
                    NotifyPropertyChange("Options");
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    NotifyPropertyChange("Title");
                }
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    NotifyPropertyChange("Message");
                }
            }
        }

        public ImageSource MessageImageSource
        {
            get { return _messageImageSource; }
            set
            {
                _messageImageSource = value;
                NotifyPropertyChange("MessageImageSource");
            }
        }

        public Visibility YesNoVisibility
        {
            get { return _yesNoVisibility; }
            set
            {
                if (_yesNoVisibility != value)
                {
                    _yesNoVisibility = value;
                    NotifyPropertyChange("YesNoVisibility");
                }
            }
        }

        public Visibility CancelVisibility
        {
            get { return _cancelVisibility; }
            set
            {
                if (_cancelVisibility != value)
                {
                    _cancelVisibility = value;
                    NotifyPropertyChange("CancelVisibility");
                }
            }
        }

        public Visibility OkVisibility
        {
            get { return _okVisibility; }
            set
            {
                if (_okVisibility != value)
                {
                    _okVisibility = value;
                    NotifyPropertyChange("OkVisibility");
                }
            }
        }

        public Visibility ContinueVisibility
        {
            get { return _continueVisibility; }
            set
            {
                if (_continueVisibility != value)
                {
                    _continueVisibility = value;
                    NotifyPropertyChange("ContinueVisibility");
                }
            }
        }

        public HorizontalAlignment ContentTextAlignment
        {
            get { return _contentTextAlignment; }
            set
            {
                if (_contentTextAlignment != value)
                {
                    _contentTextAlignment = value;
                    NotifyPropertyChange("ContentTextAlignment");
                }
            }
        }

        public FlowDirection ContentFlowDirection
        {
            get { return _contentFlowDirection; }
            set
            {
                if (_contentFlowDirection != value)
                {
                    _contentFlowDirection = value;
                    NotifyPropertyChange("ContentFlowDirection");
                }
            }
        }


        public FlowDirection TitleFlowDirection
        {
            get { return _titleFlowDirection; }
            set
            {
                if (_titleFlowDirection != value)
                {
                    _titleFlowDirection = value;
                    NotifyPropertyChange("TitleFlowDirection");
                }
            }
        }


        public bool IsOkDefault
        {
            get { return _isOkDefault; }
            set
            {
                if (_isOkDefault != value)
                {
                    _isOkDefault = value;
                    NotifyPropertyChange("IsOkDefault");
                }
            }
        }

        public bool IsContinueDefault
        {
            get { return _isContinueDefault; }
            set
            {
                if (_isContinueDefault != value)
                {
                    _isContinueDefault = value;
                    NotifyPropertyChange("IsContinueDefault");
                }
            }
        }

        public bool IsYesDefault
        {
            get { return _isYesDefault; }
            set
            {
                if (_isYesDefault != value)
                {
                    _isYesDefault = value;
                    NotifyPropertyChange("IsYesDefault");
                }
            }
        }

        public bool IsNoDefault
        {
            get { return _isNoDefault; }
            set
            {
                if (_isNoDefault != value)
                {
                    _isNoDefault = value;
                    NotifyPropertyChange("IsNoDefault");
                }
            }
        }

        public bool IsCancelDefault
        {
            get { return _isCancelDefault; }
            set
            {
                if (_isCancelDefault != value)
                {
                    _isCancelDefault = value;
                    NotifyPropertyChange("IsCancelDefault");
                }
            }
        }

        // called when the yes button is pressed
        public ICommand YesCommand
        {
            get
            {
                if (_yesCommand == null)
                    _yesCommand = new DelegateCommand(() =>
                        {
                            Result = MessageBoxResult.Yes;
                            _view.Close();
                        });
                return _yesCommand;
            }
        }

        // called when the yes button is pressed
        public ICommand ContinueCommand
        {
            get
            {
                if (_continueCommand == null)
                    _continueCommand = new DelegateCommand(() =>
                    {
                        Result = MessageBoxResult.OK;
                        _view.Close();
                    });
                return _continueCommand;
            }
        }

        // called when the no button is pressed
        public ICommand NoCommand
        {
            get
            {
                if (_noCommand == null)
                    _noCommand = new DelegateCommand(() =>
                        {
                            Result = MessageBoxResult.No;
                            _view.Close();
                        });
                return _noCommand;
            }
        }

        // called when the cancel button is pressed
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new DelegateCommand(() =>
                        {
                            Result = MessageBoxResult.Cancel;
                            _view.Close();
                        });
                return _cancelCommand;
            }
        }

        // called when the ok button is pressed
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new DelegateCommand(() =>
                        {
                            Result = MessageBoxResult.OK;
                            _view.Close();
                        });
                return _okCommand;
            }
        }

        // called when the escape key is pressed
        public ICommand EscapeCommand
        {
            get
            {
                if (_escapeCommand == null)
                    _escapeCommand = new DelegateCommand(() =>
                    {
                        switch (ButtonOption)
                        {
                            case WPFMessageBoxButton.OK:
                                Result = MessageBoxResult.OK;
                                _view.Close();
                                break;

                            case WPFMessageBoxButton.YesNoCancel:
                            case WPFMessageBoxButton.OKCancel:
                            case WPFMessageBoxButton.ContinueCancel:
                                Result = MessageBoxResult.Cancel;
                                _view.Close();
                                break;

                            case WPFMessageBoxButton.YesNo:
                                // ignore close
                                break;

                            default:
                                break;
                        }
                    });
                return _escapeCommand;
            }
        }

        // called when the form is closed by via close button click or programmatically
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new DelegateCommand(() =>
                    {
                        if (Result == MessageBoxResult.None)
                        {
                            switch (ButtonOption)
                            {
                                case WPFMessageBoxButton.OK:
                                    Result = MessageBoxResult.OK;
                                    break;

                                case WPFMessageBoxButton.YesNoCancel:
                                case WPFMessageBoxButton.OKCancel:
                                case WPFMessageBoxButton.ContinueCancel:
                                    Result = MessageBoxResult.Cancel;
                                    break;

                                case WPFMessageBoxButton.YesNo:
                                    // ignore close
                                    break;

                                default:
                                    break;
                            }
                        }
                    });
                return _closeCommand;
            }
        }

        private void SetDirections(MessageBoxOptions options)
        {
            switch (options)
            {
                case MessageBoxOptions.None:
                    ContentTextAlignment = HorizontalAlignment.Left;
                    ContentFlowDirection = FlowDirection.LeftToRight;
                    TitleFlowDirection = FlowDirection.LeftToRight;
                    break;

                case MessageBoxOptions.RightAlign:
                    ContentTextAlignment = HorizontalAlignment.Right;
                    ContentFlowDirection = FlowDirection.LeftToRight;
                    TitleFlowDirection = FlowDirection.LeftToRight;
                    break;

                case MessageBoxOptions.RtlReading:
                    ContentTextAlignment = HorizontalAlignment.Right;
                    ContentFlowDirection = FlowDirection.RightToLeft;
                    TitleFlowDirection = FlowDirection.RightToLeft;
                    break;

                case MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading:
                    ContentTextAlignment = HorizontalAlignment.Left;
                    ContentFlowDirection = FlowDirection.RightToLeft;
                    TitleFlowDirection = FlowDirection.RightToLeft;
                    break;

            }
        }

        private void NotifyPropertyChange(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void SetButtonDefault(MessageBoxResult defaultResult)
        {
            switch (defaultResult)
            {
                case MessageBoxResult.OK:
                    IsOkDefault = true;
                    IsContinueDefault = true;
                    break;

                case MessageBoxResult.Yes:
                    IsYesDefault = true;
                    break;

                case MessageBoxResult.No:
                    IsNoDefault = true;
                    break;

                case MessageBoxResult.Cancel:
                    IsCancelDefault = true;
                    break;

                default:
                    break;
            }
        }

        private void SetButtonVisibility(WPFMessageBoxButton buttonOption)
        {
            switch (buttonOption)
            {
                case WPFMessageBoxButton.YesNo:
                    OkVisibility = ContinueVisibility =CancelVisibility = Visibility.Collapsed;
                    break;

                case WPFMessageBoxButton.YesNoCancel:
                    OkVisibility = ContinueVisibility = Visibility.Collapsed;
                    break;

                case WPFMessageBoxButton.OK:
                    YesNoVisibility = ContinueVisibility = CancelVisibility = Visibility.Collapsed;
                    break;

                case WPFMessageBoxButton.OKCancel:
                    YesNoVisibility =  ContinueVisibility = Visibility.Collapsed;
                    break;

                case WPFMessageBoxButton.ContinueCancel:
                    YesNoVisibility = Visibility.Collapsed;
                    OkVisibility = Visibility.Collapsed;
                    break;

                default:
                    OkVisibility = CancelVisibility = YesNoVisibility = Visibility.Collapsed;
                    break;
            }
        }

        private void SetImageSource(MessageBoxImage image)
        {
            switch (image)
            {
                //case MessageBoxImage.Hand:
                //case MessageBoxImage.Stop:
                case MessageBoxImage.Error:
                    MessageImageSource = new BitmapImage(new Uri("pack://application:,,,/DriversGalaxy.Infrastructure;component/Images/icon-err.png"));
                    break;

                //case MessageBoxImage.Exclamation:
                case MessageBoxImage.Warning:
                    MessageImageSource = new BitmapImage(new Uri("pack://application:,,,/DriversGalaxy.Infrastructure;component/Images/icon-exclamation.png"));
                    break;

                case MessageBoxImage.Question:
                    MessageImageSource = new BitmapImage(new Uri("pack://application:,,,/DriversGalaxy.Infrastructure;component/Images/icon-question.png"));
                    break;

                //case MessageBoxImage.Asterisk:
                case MessageBoxImage.Information:
                    MessageImageSource = new BitmapImage(new Uri("pack://application:,,,/DriversGalaxy.Infrastructure;component/Images/icon-guard.png"));
                    break;

                default:
                    MessageImageSource = null;
                    break;
            }
        }
    }

    // Summary:
    //     Specifies the buttons that are displayed on a message box. Used as an argument
    //     of the Overload:System.Windows.MessageBox.Show method.
    public enum WPFMessageBoxButton
    {
        // Summary:
        //     The message box displays an OK button.
        OK = 0,
        //
        // Summary:
        //     The message box displays OK and Cancel buttons.
        OKCancel = 1,
        //
        // Summary:
        //     The message box displays Yes, No, and Cancel buttons.
        YesNoCancel = 3,
        //
        // Summary:
        //     The message box displays Yes and No buttons.
        YesNo = 4,
        //
        // Summary:
        //     The message box displays Continue and Cancel buttons.
        ContinueCancel = 5,
    }
}
