using System;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Media;

namespace CoverMaker {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = vm;
        }
        ViewModel vm = new ViewModel();
        List<string> BaseFile = new List<string>();

        const string musicdir = @"C:\音楽";
        //readonly string musicdir = @"C:\Users\heise\Desktop\test";
        const string tempdir = @"C:\Users\heise\Desktop\TEMP";
        const string waifu2xarg = @"/c D:\ソフトウェア\自作ソフト\waifu2x-caffe\waifu2x-caffe-cui -i";
        const string upRGB = "--model_dir models/upconv_7_anime_style_art_rgb";
        const string upPhoto = "--model_dir models/upconv_7_photo";

        CancellationTokenSource cts;

        private async void start_Click(object sender, RoutedEventArgs e) {
            vm.Idle = false;
            using (cts = new CancellationTokenSource()) {
                var token = cts.Token;
                var task = (vm.RenseiIsChecked) ? Task.Run(() => Rensei(token)) : Task.Run(() => Kikan(token));
                await task;
            }
            vm.Idle = true;
        }

        private void cancel_Click(object sender, RoutedEventArgs e) {
            cancel.IsEnabled = false;
            cts.Cancel();
        }

        private void Rensei(CancellationToken token) {
            if (vm.ProgressValue != 0) vm.ProgressValue = 0;
            int current = 0;
            BaseFile.Clear();
            Scan(musicdir);

            int total = BaseFile.Count;
            vm.ProcessString = $"0 / {total}";
            Directory.CreateDirectory(tempdir);
            try {
                for (int i = 0; i < total; i++) {
                    token.ThrowIfCancellationRequested();
                    string waifusrcpath = Path.Combine(tempdir, i.ToString());
                    Directory.CreateDirectory(waifusrcpath);
                    string filename = Path.GetFileName(BaseFile[i]);
                    string filenamewoextension = Path.GetFileNameWithoutExtension(BaseFile[i]);
                    string waifusrc = Path.Combine(waifusrcpath, filename);
                    File.Copy(BaseFile[i], waifusrc);
                    Waifu2x(waifusrc, filenamewoextension);
                    current++;
                    vm.ProgressValue = (double)current / total;
                    vm.ProcessString = $"{current} / {total}";
                }

                SystemSounds.Asterisk.Play();
                MessageBox.Show("完成しました");
                vm.ProcessString = null;
                vm.ProgressValue = 0;

            }
            catch (Exception ex) {
                SystemSounds.Asterisk.Play();
                MessageBox.Show(ex.Message);
                BaseFile.Clear();
            }

        }

        private void Kikan(CancellationToken token) {
            if (vm.ProgressValue != 0) vm.ProgressValue = 0;
            int current = 0;
            int total = BaseFile.Count;
            vm.ProcessString = $"0 / {total}";

            try {
                for (int i = 0; i < total; i++) {
                    token.ThrowIfCancellationRequested();
                    string a = Path.Combine(tempdir, i.ToString(), "Folder.jpg");
                    string b = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(BaseFile[i]).FullName).FullName).FullName, "Folder.jpg");
                    File.Move(a, b);
                    current++;
                    vm.ProgressValue = (double)current / total;
                    vm.ProcessString = $"{current} / {total}";
                }

                SystemSounds.Asterisk.Play();
                MessageBox.Show("完成しました");
                vm.ProcessString = null;
                vm.ProgressValue = 0;
                BaseFile.Clear();
            }
            catch (Exception ex) {
                SystemSounds.Asterisk.Play();
                MessageBox.Show(ex.Message);
                BaseFile.Clear();
            }
        }



        private void Scan(string src) {
            foreach (string basedir in Directory.EnumerateDirectories(src, "ベース")) {
                foreach (string basefile in Directory.EnumerateFiles(basedir).Where(s => s.EndsWith(".jpg") || s.EndsWith(".webp"))) {
                    BaseFile.Add(basefile);
                }
            }
            #region コピー元のディレクトリにあるディレクトリについて、再帰的に呼び出す
            foreach (string dir in Directory.EnumerateDirectories(src)) Scan(dir);

            #endregion
        }

        private void Waifu2x(string srcfile, string name) {

            if (name == "photobase") //名前を変更するだけ
            {
                string outputfile = Path.Combine(Directory.GetParent(srcfile).FullName, "Folder" + Path.GetExtension(srcfile));
                File.Move(srcfile, outputfile);

            }
            else {
                string arg = "";
                string outputfile = Path.Combine(Directory.GetParent(srcfile).FullName, "Folder.png");
                string lastarg = $" -t 1 -o {outputfile.WQ()}";
                if (name == "photobase_sx2") {
                    arg = $"{waifu2xarg} {srcfile.WQ()} {upPhoto} -m scale -s 2 {lastarg}";
                }
                else if (name.Substring(0, 7) == "base_ns") {
                    string[] num = name.Split(new string[] { "base_ns" }, StringSplitOptions.RemoveEmptyEntries)[0].Split('x');
                    string noiselevel = num[0];
                    string scaleratio = num[1];

                    arg = waifu2xarg + srcfile.WQ() + upRGB + " -n " + noiselevel + " -s " + scaleratio + lastarg;
                }
                else//base_n
                {
                    string noiselevel = name.Split(new string[] { "base_n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    arg = waifu2xarg + srcfile.WQ() + upRGB + " -m noise -n " + noiselevel + lastarg;
                }
                StartProcess(arg);
                File.Delete(srcfile);
            }
        }

        private void StartProcess(string arg) {
            using (Process p = new Process()) {
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = arg;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
            }
        }
    }
}
