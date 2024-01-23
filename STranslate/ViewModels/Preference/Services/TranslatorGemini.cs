﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorGemini : ObservableObject, ITranslator
    {
        public TranslatorGemini()
            : this(Guid.NewGuid(), "https://generativelanguage.googleapis.com", "Gemini") { }

        public TranslatorGemini(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Gemini,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.GeminiService
        )
        {
            Identify = guid;
            Url = url;
            Name = name;
            Icon = icon;
            AppID = appID;
            AppKey = appKey;
            IsEnabled = isEnabled;
            Type = type;
        }

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private ServiceType _type = 0;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Gemini;

        [JsonIgnore]
        [ObservableProperty]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public object _data = string.Empty;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        #region Show/Hide Encrypt Info

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _keyHide = true;

        private void ShowEncryptInfo() => KeyHide = !KeyHide;

        private RelayCommand? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand(new Action(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        public async Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(AppKey))
                throw new Exception("请先完善配置");
            if (request is string[] strs)
            {
                var source = strs[0];
                var target = strs[1];
                var content = strs[2];
                UriBuilder uriBuilder = new(Url);

                if (!uriBuilder.Path.EndsWith("/v1beta/models/gemini-pro:streamGenerateContent"))
                {
                    uriBuilder.Path = "/v1beta/models/gemini-pro:streamGenerateContent";
                }

                uriBuilder.Query = $"key={AppKey}";

                // 组织语言
                var a_content = source.Equals("auto", StringComparison.CurrentCultureIgnoreCase)
                    ? $"Translate the following text to {target}: {content}"
                    : $"Translate the following text from {source} to {target}: {content}";

                // 构建请求数据
                var reqData = new { contents = new[] { new { parts = new[] { new { text = a_content } } } } };

                // 为了流式输出与MVVM还是放这里吧
                var jsonData = JsonConvert.SerializeObject(reqData);

                await HttpUtil.PostAsync(uriBuilder.Uri, jsonData, null, msg => OnDataReceived?.Invoke(msg), token);
            }
        }

        public Task<object> TranslateAsync(object request, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
