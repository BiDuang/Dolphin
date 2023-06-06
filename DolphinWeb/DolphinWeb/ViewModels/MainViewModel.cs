using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace DolphinWeb.ViewModels;

public class MainViewModel : ViewModelBase
{
    private string? _username;
    private string? _password;
    private string? _email;
    private bool _before1Day;
    private bool _before3Day;
    private bool _before7Day;
    private string? _uid;

    public string? Username
    {
        get => _username;
        set
        {
            if (value != null && !value.All(char.IsDigit))
                value = value.Length > 0 ? _username : null;

            this.RaiseAndSetIfChanged(ref _username, value);
        }
    }

    public string? Password
    {
        get => _password;
        set
        {
            if (value != null && !value.All(char.IsAscii))
                value = value.Length > 0 ? _password : null;

            this.RaiseAndSetIfChanged(ref _password, value);
        }
    }

    public string? Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    public bool Before1Day
    {
        get => _before1Day;
        set => this.RaiseAndSetIfChanged(ref _before1Day, value);
    }

    public bool Before3Day
    {
        get => _before3Day;
        set => this.RaiseAndSetIfChanged(ref _before3Day, value);
    }

    public bool Before7Day
    {
        get => _before7Day;
        set => this.RaiseAndSetIfChanged(ref _before7Day, value);
    }

    private string? _userInfo;

    public string? UserInfo
    {
        get => _userInfo;
        set => this.RaiseAndSetIfChanged(ref _userInfo, value);
    }


    private bool _isLoginPanelVisible = true;
    private bool _isProfilePanelVisible;
    private bool _isProcessingPanelVisible;
    private bool _isSuccessPanelVisible;
    private bool _isPanicPanelVisible;
    private bool _isQuitPanelVisible;
    private bool _isWrongPasswordPanelVisible;
    private bool _isInfoPanelVisible;

    public bool IsLoginPanelVisible
    {
        get => _isLoginPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isLoginPanelVisible, value);
    }

    public bool IsProfilePanelVisible
    {
        get => _isProfilePanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isProfilePanelVisible, value);
    }

    public bool IsProcessingPanelVisible
    {
        get => _isProcessingPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isProcessingPanelVisible, value);
    }

    public bool IsSuccessPanelVisible
    {
        get => _isSuccessPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isSuccessPanelVisible, value);
    }

    public bool IsPanicPanelVisible
    {
        get => _isPanicPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isPanicPanelVisible, value);
    }

    public bool IsQuitPanelVisible
    {
        get => _isQuitPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isQuitPanelVisible, value);
    }

    public bool IsWrongPasswordPanelVisible
    {
        get => _isWrongPasswordPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isWrongPasswordPanelVisible, value);
    }
    
    public bool IsInfoPanelVisible
    {
        get => _isInfoPanelVisible;
        set => this.RaiseAndSetIfChanged(ref _isInfoPanelVisible, value);
    }

    public ICommand OnLoginCommand { get; }
    public ICommand OnResetCommand { get; }
    public ICommand OnRegisterCommand { get; }

    public ICommand OnQuitCommand { get; }
    public ICommand OnInfoCommand { get; }

    public MainViewModel()
    {
        OnLoginCommand = ReactiveCommand.Create(async () =>
        {
            if (Username == null || Password == null)
                return;

            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = true;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;

            await GetUserInfo();
        });

        OnResetCommand = ReactiveCommand.Create(() =>
        {
            Username = null;
            Password = null;
            _userInfo = null;
            _icsUri = string.Empty;
            IsLoginPanelVisible = true;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;
        });

        OnRegisterCommand = ReactiveCommand.Create(async () =>
        {
            if (Email == null || !Email.Contains('@') || !Email.Contains('.'))
                return;

            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = true;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;

            await RegisterUser();
        });

        OnQuitCommand = ReactiveCommand.Create(async () =>
        {
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = true;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;

            await QuitDolphin();
        });

        OnInfoCommand = ReactiveCommand.Create(() =>
        {
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = true;
            Process.Start(new ProcessStartInfo("https://github.com/BiDuang/Dolphin")
            {
                UseShellExecute = true
            });
        });
    }

    private string _icsUri = string.Empty;

    private async Task GetUserInfo()
    {
        try
        {
            var client = new HttpClient();
            var content = new StringContent(
                $"{{\"username\":\"{Username}\",\"password\":\"{Password}\"}}",
                Encoding.UTF8, "application/json");
            var response = await client.PostAsync(new Uri("https://ouc.114514.bid/homework"), content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                IsLoginPanelVisible = false;
                IsProfilePanelVisible = false;
                IsProcessingPanelVisible = false;
                IsSuccessPanelVisible = false;
                IsPanicPanelVisible = false;
                IsQuitPanelVisible = false;
                IsWrongPasswordPanelVisible = true;
                IsInfoPanelVisible = false;
                return;
            }

            response.EnsureSuccessStatusCode();
            var responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            _icsUri = (string)responseBody["data"]![0]!;
            UserInfo = "姓名：" + (string)responseBody["data"]![1]!["name"]! + "\n" +
                       "学号：" + (string)responseBody["data"]![1]!["uid"]! + "\n" +
                       "学院：" + (string)responseBody["data"]![1]!["college"]! + "\n" +
                       "专业：" + (string)responseBody["data"]![1]!["major"]! + "\n";
            _uid = (string)responseBody["data"]![1]!["uid"]!;

            Before1Day = false;
            Before3Day = false;
            Before7Day = false;
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = true;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;
        }
        catch
        {
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = true;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;
        }
    }

    private async Task RegisterUser()
    {
        try
        {
            var client = new HttpClient();
            var noticeDays = new List<int>();
            if (Before1Day)
                noticeDays.Add(1);
            if (Before3Day)
                noticeDays.Add(3);
            if (Before7Day)
                noticeDays.Add(7);

            var noticedInfo = new List<List<string>>();
            for (var i = 0; i < 3; i++)
            {
                noticedInfo.Add(new List<string>());
            }

            var content = new StringContent(
                $"{{\"uid\":\"{_uid}\", \"email\":\"{Email}\",\"uri\":\"{_icsUri}\",\"notice_day\":{JsonConvert.SerializeObject(noticeDays)}," +
                $"\"noticed_info\":{JsonConvert.SerializeObject(noticedInfo)}}}",
                Encoding.UTF8, "application/json");
            var response = await client.PostAsync(new Uri("https://ouc.114514.bid/register"), content);
            response.EnsureSuccessStatusCode();
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = true;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = false;
            IsInfoPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
        }
        catch
        {
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = true;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;
        }
    }

    private async Task QuitDolphin()
    {
        try
        {
            var client = new HttpClient();
            var response = await client.DeleteAsync(new Uri($"https://ouc.114514.bid/quit?uid={_uid}"));
            response.EnsureSuccessStatusCode();
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = false;
            IsQuitPanelVisible = true;
            IsInfoPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
        }
        catch
        {
            IsLoginPanelVisible = false;
            IsProfilePanelVisible = false;
            IsProcessingPanelVisible = false;
            IsSuccessPanelVisible = false;
            IsPanicPanelVisible = true;
            IsQuitPanelVisible = false;
            IsWrongPasswordPanelVisible = false;
            IsInfoPanelVisible = false;
        }
    }
}