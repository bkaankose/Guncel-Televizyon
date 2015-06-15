using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SM.Media;
using SM.Media.Utility;
using SM.Media.Web;
using System.Diagnostics;
using Windows.UI.Core;
using System.Text;
using GTVWin8.DataModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using GTVWin8.Interfaces;
using Windows.UI;
using SM.Media.Playlists;
using System.Threading.Tasks;
using GTVWin8.ViewModels;
namespace GTVWin8
{
    public sealed partial class WatchPage : Page
    {

        #region PhoneSM
        static readonly TimeSpan StepSize = TimeSpan.FromMinutes(2);
        static readonly IApplicationInformation ApplicationInformation = ApplicationInformationFactory.DefaultTask.Result;
        readonly IHttpClients _httpClients;
        readonly IMediaElementManager _mediaElementManager;
        readonly DispatcherTimer _positionSampler;
        IMediaStreamFascade _mediaStreamFascade;
        TimeSpan _previousPosition;
        private ISubProgram[] Bandwiths;
        #endregion

        private bool streamInitializing = false;
        private bool streamsOpen = false;
        private bool channelControlOpen = false;
        private GTVCore appCore;
        private WatchPageViewModel WatchModel = new WatchPageViewModel();
        public WatchPage()
        {
            this.InitializeComponent();
            _mediaElementManager = new WinRtMediaElementManager(Dispatcher,
                () =>
                {
                    UpdateState(MediaElementState.Opening);

                    return StreamElement;
                },
                me => UpdateState(MediaElementState.Closed));

            var userAgent = ApplicationInformation.CreateUserAgent();

            _httpClients = new HttpClients(userAgent: userAgent);

            _positionSampler = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(75)
            };
            _positionSampler.Tick += OnPositionSamplerOnTick;

            Unloaded += (sender, args) => OnUnload();
            
            cvs.Source = MainPage.queries;
        }

        private async Task<string> UpdateChannelURL(int Id)
        {
            if (appCore.allChannels.FirstOrDefault(a => a.Id == Id).IsSpecial)
                return await appCore.getSpecialURL(Id);
            else
                return appCore.allChannels.FirstOrDefault(x => x.Id == Id).Url;
        }
        private void StreamElement_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            var state = null == StreamElement ? MediaElementState.Closed : StreamElement.CurrentState;

            if (null != _mediaStreamFascade)
            {
                var managerState = _mediaStreamFascade.State;

                if (MediaElementState.Closed == state)
                {
                    if (TsMediaManager.MediaState.OpenMedia == managerState || TsMediaManager.MediaState.Opening == managerState || TsMediaManager.MediaState.Playing == managerState)
                        state = MediaElementState.Opening;
                }
            }   
            UpdateState(state);
        }
        private void setProgress(bool Running = false, string Content = "")
        {
            midProgress.IsBusy = Running;
            midProgress.Content = Content;
        }
        void UpdateState(MediaElementState state)
        {
            if (MediaElementState.Buffering == state && null != StreamElement)
                setProgress(true,string.Format("Yükleniyor {0:F2}%", StreamElement.BufferingProgress * 100));
            switch (state)
            {
                case MediaElementState.Opening:
                    setProgress(true,"Kanal Yükleniyor ...");
                    break;
                case MediaElementState.Closed:
                    if (streamInitializing || ChannelChanging)
                        setProgress(true, "Kanal Yükleniyor ...");
                    else
                        setProgress(false, "Kanal yüklenemedi\nLütfen kanalın çalışmadığını düşünüyorsanız rapor gönderek daha hızlı düzeltilmesi için\ngeliştiriciye mesaj gönderebilirsiniz.");
                    break;
                case MediaElementState.Playing:
                    setProgress(false);
                    break;
            }

            OnPositionSamplerOnTick(null, null);
        }
        void OnPositionSamplerOnTick(object o, object o1)
        {
            if (StreamElement == null) return;
            var positionSample = StreamElement.Position;

            if (positionSample == _previousPosition)
                return;

            _previousPosition = positionSample;
        }
        string FormatTimeSpan(TimeSpan timeSpan)
        {
            var sb = new StringBuilder();
            if (timeSpan < TimeSpan.Zero)
            {
                sb.Append('-');
                timeSpan = -timeSpan;
            }
            if (timeSpan.Days > 1)                sb.AppendFormat(timeSpan.ToString(@"%d\."));

            sb.Append(timeSpan.ToString(@"hh\:mm\:ss\.ff"));
            return sb.ToString();
        }
        public void PlayHLS(string HLSUrl)
        {
            /* Play M3U8s in MediaElement */
            InitializeMediaStream();
            streamInitializing = true;
            PlaylistSegmentManagerPolicy.SelectSubProgram = programs  =>
            {
                Bandwiths = programs
                .Where(p => p.Bandwidth < 7000000 && (!p.Width.HasValue || p.Width.Value <= 1920))
                .ToArray();
                if (WatchModel.SelectedBandwith != null)
                    if (Bandwiths.FirstOrDefault(x => x.Bandwidth == WatchModel.SelectedBandwith.Bandwidth) != null) // Quality changed
                    {
                        ModifyBandwithText(ConvertBytesToMegabytes(long.Parse((Math.Round((double)WatchModel.SelectedBandwith.Bandwidth)).ToString())).ToString() + " mbit/s").Wait();
                        return WatchModel.SelectedBandwith;
                    }
                    else
                        return Bandwiths[Bandwiths.Length / 2];
                else
                {
                    InitializeBandwiths().Wait();
                    return Bandwiths.Length < 1 ? null : Bandwiths[Bandwiths.Length / 2];
                }
            };

            if (_mediaStreamFascade == null) return;
            HLSUrl = HLSUrl.Replace("\"", string.Empty);
            _mediaStreamFascade.Source = new Uri(HLSUrl, UriKind.Absolute);
            StreamElement.Play();
            _positionSampler.Start();
            streamInitializing = false;
        }
        private async Task ModifyBandwithText(string nText)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lblSelectedQuality.Text = nText;
            });
        }
        private async Task InitializeBandwiths()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                flyoutBandwiths.Items.Clear();
                if (Bandwiths.Length > 1)
                {
                    foreach (var bandW in Bandwiths)
                        AddNewBandwithToFlyout(bandW.Bandwidth);

                    lblSelectedQuality.Text = ConvertBytesToMegabytes(Bandwiths[Bandwiths.Length / 2].Bandwidth) + " mbit/s";
                }
                else
                {
                    AddNewBandwithToFlyout();
                    lblSelectedQuality.Text = "Standart";
                }
                    
            });
        }
        private void AddNewBandwithToFlyout(long bandwithVal = 0)
        {
            var tmpBandMenuItem = new MenuFlyoutItem();
            tmpBandMenuItem.Click += tmpBandMenuItem_Click;
            if (bandwithVal != 0)
                tmpBandMenuItem.Text = ConvertBytesToMegabytes(long.Parse((Math.Round((double)bandwithVal)).ToString())).ToString() + " mbit/s";
            else
                tmpBandMenuItem.Text = "Standart";

            flyoutBandwiths.Items.Add(tmpBandMenuItem);
        }

        void tmpBandMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sent = sender as MenuFlyoutItem;
            WatchModel.SelectedBandwith = Bandwiths[flyoutBandwiths.Items.IndexOf(sent)];
            StopStream();
            PlayHLS(appCore.SelectedChannel.Url);
        }
        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1000f) / 1000f;
        }
        private void StopStream()
        {
            if (null != StreamElement)
                StreamElement.Source = null;
            _positionSampler.Stop();
        }
        private void PlayMMS(string MMSUrl)
        {
            StreamElement.Source = new Uri(MMSUrl, UriKind.Absolute);
        }
        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            appCore = e.Parameter as GTVCore;
            WatchModel.AllCurrentStreams = appCore.SelectedChannel.AllCurrentPrograms;
            this.DataContext = WatchModel;
            await PlayChannel(appCore.SelectedChannel);


            //StreamElement.Source = new Uri("mms://sinema.canlitv.mobi/sinema1", UriKind.Absolute);
            //await PlayChannel(new Channels() { Url = "http://cine5.mobil.cubecdn.net/cine5/stream_1.m3u8" });

            StreamElement.Play();
            _positionSampler.Start();
        }
        private async Task PlayChannel(Channels nChannel)
        {
            setProgress(true, "Kanal Yükleniyor ...");
            lblSelectedQuality.Text = "...";
            StopStream();
            if (Bandwiths != null)
                Array.Clear(Bandwiths, 0, Bandwiths.Length);

            appCore.SelectedChannel.Url = await UpdateChannelURL(appCore.SelectedChannel.Id);
            if (appCore.SelectedChannel.Url == null || appCore.SelectedChannel.Url == "\"empty\"")
            {
                var reportRes = await appCore.postReport(appCore.SelectedChannel.Id);
                if(reportRes)
                    await GTVDialogs.ShowDialog("Yayın bilgisine ulaşılamadı , kanal hata raporu otomatik olarak gönderilmiştir !", "Hata :(");

                if (Frame.CanGoBack)
                    Frame.GoBack();

                return;
            }
            if (appCore.SelectedChannel.Url.Contains(".mms"))
                PlayMMS(nChannel.Url);
            else
                PlayHLS(nChannel.Url);

            
        }
        private async Task ReloadStreams()
        {
            if(!appCore.SelectedChannel.CanListStreams && streamsOpen)
            {
                closeStreams.Begin();
                streamsOpen = false;
                return;
            }

            if (WatchModel.AllCurrentStreams == null)
                WatchModel.AllCurrentStreams = new ObservableCollection<ChannelStreamProgram>();
            else
                WatchModel.AllCurrentStreams.Clear();

            var latestLoadedStreams = await appCore.getChannelStream(appCore.SelectedChannel.Id);
            if (latestLoadedStreams == null) return;
            foreach (var item in latestLoadedStreams) WatchModel.AllCurrentStreams.Add(item);
        }
        void InitializeMediaStream()
        {
            if (null != _mediaStreamFascade)
                return;

            _mediaStreamFascade = MediaStreamFascadeSettings.Parameters.Create(_httpClients, _mediaElementManager.SetSourceAsync);
            _mediaStreamFascade.SetParameter(_mediaElementManager);
            
            _mediaStreamFascade.StateChange += TsMediaManagerOnStateChange;
        }
        async void CleanupMediaStream()
        {
            if (null == _mediaStreamFascade) return;
            _mediaStreamFascade.StateChange -= TsMediaManagerOnStateChange;
            _positionSampler.Stop();
            _positionSampler.Tick -= OnPositionSamplerOnTick;
            await _mediaElementManager.CloseAsync();
            _mediaStreamFascade = null;
            await _mediaStreamFascade.DisposeAsync();
        }
        void TsMediaManagerOnStateChange(object sender, TsMediaManagerStateEventArgs tsMediaManagerStateEventArgs)
        {
            var awaiter = Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                var message = tsMediaManagerStateEventArgs.Message;

                if (!string.IsNullOrWhiteSpace(message))
                    setProgress(true, message);

                StreamElement_CurrentStateChanged(null, null);
            });
        }
        private void StreamElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            CleanupMediaStream();
            setProgress(false, "Kanal yüklenemiyor \nMenüden kanal ile ilgili rapor göndererek daha hızlı düzeltilmesini sağlayabilirsiniz !");
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            CleanupMediaStream();
        }
        void StopMedia()
        {
            if (null != StreamElement)
                StreamElement.Source = null;

            _positionSampler.Stop();
        }
        private void StreamElement_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            StreamElement_CurrentStateChanged(sender, e);
        }
        public void OnUnload()
        {
            if (null != StreamElement)
                StreamElement.Source = null;

            var mediaStreamFascade = _mediaStreamFascade;
            _mediaStreamFascade = null;

        }
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (WatchModel.Volume == 0)
                WatchModel.Volume = 100;
            else
                WatchModel.Volume = 0;
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void btnStreams_Click(object sender, RoutedEventArgs e)
        {
            if (appCore.SelectedChannel.CanListStreams && appCore.SelectedChannel.AllCurrentPrograms == null)
            {
                var msg = new MessageDialog("Yayın akışı mevcut fakat henüz yüklenmedi.Birkaç saniye sonra tekrar deneyin.");
                await msg.ShowAsync();
                closeStreams.Begin();
                streamsOpen = false;
                
                return;
            }
            else if (!appCore.SelectedChannel.CanListStreams)
            {
                
                var msg = new MessageDialog("Bu kanal için yayın akışı desteği bulunmamaktadır.");
                await msg.ShowAsync();
                return;
            }
            if (streamsOpen)
                closeStreams.Begin();
            else
                openStreams.Begin();

            streamsOpen = !streamsOpen;
        }
        private async void btnReport_Click(object sender, RoutedEventArgs e)
        {
            
            var msg = new MessageDialog(appCore.SelectedChannel.Name + " kanalını yüklenmediği için raporlamak üzeresiniz.\nBu işlem kanalın daha hızlı düzeltilmesi için geliştiriciye mesaj gönderecektir.\nDevam etmek istiyor musunuz ?");
            msg.Commands.Add(new UICommand("Evet",new UICommandInvokedHandler(this.CommandInvokedHandler)));
            msg.Commands.Add(new UICommand("Hayır"));
            msg.CancelCommandIndex = 1;
            msg.DefaultCommandIndex = 0;
            await msg.ShowAsync();
        }
        private async void CommandInvokedHandler(IUICommand command)
        {
            if(command.Label == "Evet")
            {
                var res = await appCore.postReport(appCore.SelectedChannel.Id);
                if(res)
                    await GTVDialogs.ShowDialog("Rapor başarıyla iletildi !","Ileti Raporu");
            }
        }

        private void btnChannelControl_Click(object sender, RoutedEventArgs e)
        {
            if (!channelControlOpen)
                channelsControlOpen.Begin();
            else
                channelsControlClose.Begin();

            channelControlOpen = !channelControlOpen;
            this.BottomAppBar.IsOpen = false;
        }
        private bool ChannelChanging = false;
        private async void channelChangeList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ChannelChanging = true;
            var sClicked = e.ClickedItem as Channels;
            appCore.SelectedChannel = sClicked;
            await ReloadStreams();
            await PlayChannel(sClicked);
            this.BottomAppBar.IsOpen = false;
            channelsControlClose.Begin();
            channelControlOpen = false;
            ChannelChanging = false;
        }
    }
}
