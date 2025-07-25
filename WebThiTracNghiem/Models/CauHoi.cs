﻿using Newtonsoft.Json;

namespace WebThiTracNghiem.Models
{
	public class CauHoi
	{
		public int Id { get; set; }
		public string NoiDung { get; set; }
		public string Loai {  get; set; }
		public int DeThiId { get; set; }
		[JsonIgnore]
		public virtual DeThi DeThi { get; set; }
		public List<DapAn> DapAnList { get; set; } = new();

	}
}
