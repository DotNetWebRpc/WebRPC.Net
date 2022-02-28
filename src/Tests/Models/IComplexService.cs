using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRPC.Models
{
    public interface IComplexService
    {
        Task Complex1();
        Task<bool> Complex2();
        Task<DateTime> Complex3();
        Task<DateTime> Complex4();
        Task<int> Complex5(string s1, out int i);
        Task<int> Complex6(string s1, out int i, List<string> messages);
        Task<string[]> Complex7(string s1, out int i, string[] messages);
        Task<int> Complex8(string s1, out int i, out string[] messages);
        Task<int> Complex9(string s1, out int i, out Dictionary<int, string> pairs);
        Task<Dictionary<int, string>> Complex10(string s1, ref int i, out Dictionary<int, string> pairs);
    }
}
