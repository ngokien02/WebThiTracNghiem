namespace WebThiTracNghiem.Models
{
    public class ChuDe
    {
        public int Id { get; set; }
        public string TenCD { get; set; }
        public ICollection<CauHoi> CauHoiList { get; set; }
    }
}
