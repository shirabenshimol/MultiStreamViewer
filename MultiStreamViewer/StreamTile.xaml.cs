using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace MultiStreamViewer
{
    public partial class StreamTile : UserControl
    {
        private static bool _vlcInited;
        private static LibVLC? _libVLC;

        private VlcMediaPlayer? _player;
        private Media? _currentMedia;
        private bool _isPaused;

        public StreamTile()
        {
            InitializeComponent();
            EnsureVlc();

            _player = new VlcMediaPlayer(_libVLC!);

            Loaded += (_, __) => VideoView.MediaPlayer = _player;
            Unloaded += (_, __) => Cleanup();

            WirePlayerEventsOnce();

            SetNoSignal("NO SIGNAL");
        }

        private static void EnsureVlc()
        {
            if (_vlcInited) return;

            Core.Initialize();

            _libVLC = new LibVLC(
                "--no-video-title-show",
                "--quiet",
                "--avcodec-hw=none"
            );

            _vlcInited = true;
        }

        public void SetUrl(string url) => UrlBox.Text = url;

        public void PlayCurrent() => Connect();

        public void Connect()
        {
            var input = (UrlBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                SetNoSignal("EMPTY URL");
                return;
            }

            try
            {
                SetPending("CONNECTING...");
                _isPaused = false;
                ReplaceMedia(null);
                Media media;

                if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
                {
                    // RTSP / URL
                    media = new Media(_libVLC!, uri);
                    media.AddOption(":network-caching=300");
                }
                else
                {
                    // קובץ
                    var fullPath = Path.GetFullPath(input);
                    if (!File.Exists(fullPath))
                    {
                        SetNoSignal("FILE NOT FOUND");
                        return;
                    }

                    media = new Media(_libVLC!, new Uri(fullPath));
                    media.AddOption(":file-caching=300");
                }

                media.AddOption(":avcodec-hw=none");

                ReplaceMedia(media);

                _player!.Play(_currentMedia);
            }
            catch
            {
                SetNoSignal("FAILED");
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e) => Connect();

        private void UrlBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Connect();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (_player == null) return;

            // Toggle Pause/Resume
            if (_player.IsPlaying)
            {
                _player.SetPause(true);
                _isPaused = true;
            }
            else
            {
                _player.SetPause(false);
                if (!_player.IsPlaying) _player.Play();
                _isPaused = false;
            }

            PauseBtn.Content = _isPaused ? "Resume" : "Pause";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StopHard();
        }

        public void StopHard()
        {
            if (_player == null) return;

            try
            {
                _player.Stop();
                _player.Mute = true;     
                _player.Mute = false;
            }
            catch { }

            _isPaused = false;
            PauseBtn.Content = "Pause";

            SetNoSignal("STOPPED");
        }

        private void WirePlayerEventsOnce()
        {
            if (_player == null) return;

            _player.Playing += (_, __) => Dispatcher.Invoke(() => SetLive("LIVE"));
            _player.Paused += (_, __) => Dispatcher.Invoke(() => SetPaused("PAUSED"));
            _player.Stopped += (_, __) => Dispatcher.Invoke(() => SetNoSignal("STOPPED"));
            _player.EndReached += (_, __) => Dispatcher.Invoke(() => SetNoSignal("ENDED"));
            _player.EncounteredError += (_, __) => Dispatcher.Invoke(() => SetNoSignal("FAILED"));
            _player.Buffering += (_, e) =>
                Dispatcher.Invoke(() =>
                {
                    if (!_player.IsPlaying)
                        SetPending($"BUFFERING {e.Cache:0}%");
                });
        }

        private void ReplaceMedia(Media? newMedia)
        {
            try { _player?.Stop(); } catch { }

            if (_currentMedia != null)
            {
                try { _currentMedia.Dispose(); } catch { }
                _currentMedia = null;
            }

            _currentMedia = newMedia;
        }

        private void Cleanup()
        {
            try { _player?.Stop(); } catch { }
            try { _currentMedia?.Dispose(); } catch { }
            _currentMedia = null;

            try { _player?.Dispose(); } catch { }
            _player = null;
        }

        private void SetLive(string text)
        {
            Overlay.Text = "";
            StatusText.Text = text;
            Frame.BorderBrush = Brushes.LimeGreen;
        }

        private void SetPaused(string text)
        {
            Overlay.Text = "";
            StatusText.Text = text;
            Frame.BorderBrush = Brushes.Gold;
        }

        private void SetPending(string text)
        {
            Overlay.Text = "";
            StatusText.Text = text;
            Frame.BorderBrush = Brushes.DodgerBlue;
        }

        private void SetNoSignal(string text)
        {
            Overlay.Text = text;
            StatusText.Text = text;
            Frame.BorderBrush = Brushes.IndianRed;
        }

        private void UrlPreview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UrlPreview.Visibility = Visibility.Collapsed;
            UrlBox.Visibility = Visibility.Visible;
            UrlBox.Focus();
            UrlBox.SelectAll();
        }

        private void UrlBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            UrlBox.Visibility = Visibility.Collapsed;
            UrlPreview.Visibility = Visibility.Visible;
        }
    }
}