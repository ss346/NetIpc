using NetIpc.Common.Interfaces;
using Newtonsoft.Json;

namespace NetIpc.Common
{
    public class IpcDataPack : IIpcDataPack
    {
        [JsonProperty]
        private string _json;

        public T GetData<T>()
        {
            return _json == null ? default : JsonConvert.DeserializeObject<T>(_json);
        }
        public void SetData<T>(T data)
        {
            _json = data == null ? null : JsonConvert.SerializeObject(data);
        }
    }
}
