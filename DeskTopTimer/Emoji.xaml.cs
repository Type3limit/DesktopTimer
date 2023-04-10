using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeskTopTimer
{
    /// <summary>
    /// Emoji.xaml 的交互逻辑
    /// </summary>
    public partial class EmojiWindow : MahApps.Metro.Controls.MetroWindow 
    {
        MainWorkSpace? viewModel = null;
        private bool isClosed  = false;
        public bool IsClosed
        {
            get=>isClosed;
            private set=>isClosed = value;
        }
        public EmojiWindow()
        {
            InitializeComponent();
            DataContextChanged += OptionsWindow_DataContextChanged;
            Closed += OptionsWindow_Closed;
            InPutText.Focus();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            WindowClose();
        }

        private void OptionsWindow_Closed(object? sender, EventArgs e)
        {
            IsClosed = true;
            if (viewModel != null)
            {
                viewModel.ShouldOpenEmojiResult = false;
            }

        }

        private void OptionsWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = e.NewValue as MainWorkSpace;
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //viewModel?.RunTranslateCommand?.Execute(InPutText.Text);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            
            if(e.Key== Key.Enter)
            {
                if(viewModel?.SelectedEmoji!=null)
                {
                    //DataObject dataObject = new DataObject();
                    //dataObject.SetImage(viewModel?.SelectedEmoji?.imageSource);
                    //Clipboard.SetDataObject(dataObject);
                    Clipboard.SetImage(viewModel?.SelectedEmoji?.imageSource);
                }
                WindowClose();
            }
            else if(e.Key==Key.Up)
            {
                var index = viewModel?.EmojiResults.IndexOf(viewModel?.SelectedEmoji);
                if(index>=0)
                {
                    viewModel.SelectedEmoji = viewModel.EmojiResults.ElementAt((int)((index-1<0?0:index-1) % viewModel.EmojiResults.Count));
                }
                if(index >=(viewModel.EmojiResults.Count-3))
                {
                    viewModel?.RunEmojiRequest();
                }
            }
            else if(e.Key==Key.Down)
            {
                var index = viewModel?.EmojiResults.IndexOf(viewModel?.SelectedEmoji);
                if (index >= 0)
                {
                    viewModel.SelectedEmoji = viewModel.EmojiResults.ElementAt((int)((index + 1 ) % viewModel.EmojiResults.Count));

                }
                if(index >=(viewModel.EmojiResults.Count-3))
                {
                    viewModel?.RunEmojiRequest();
                }
            }
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel?.SelectedEmoji != null)
            {
                //DataObject dataObject = new DataObject();
                //    dataObject.SetImage(viewModel?.SelectedEmoji?.imageSource);
                //    Clipboard.SetDataObject(dataObject);
                 Clipboard.SetImage(viewModel?.SelectedEmoji?.imageSource);
            }
            WindowClose();
        }
        bool IsInClose = false;
        public void WindowClose()
        {
            if(IsInClose)
                return;
            IsInClose = true;
            Close();
        }
    }
}
