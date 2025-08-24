// ExamManager.js — bản rút gọn (chỉ giữ Export + Filter)
(() => {
  const qs  = (sel, root=document) => root.querySelector(sel);
  const qsa = (sel, root=document) => Array.from(root.querySelectorAll(sel));

  let examStatusFilter, examSearchInput, examForm;

  function init() {
    // Kiểm tra có đúng trang quản lý kỳ thi không
    const onRightPage = document.getElementById('examStatusFilter') || document.getElementById('examForm');
    if (!onRightPage) return;

    examStatusFilter = document.getElementById('examStatusFilter');
    examSearchInput  = document.getElementById('examSearchInput');
    examForm         = document.getElementById('examForm');

    // Bộ lọc
    examStatusFilter && examStatusFilter.addEventListener('change', filterExams);
    examSearchInput  && examSearchInput.addEventListener('input', filterExams);

    // Nút export
    const btnExport = document.getElementById('btnExportExam');
    btnExport && btnExport.addEventListener('click', (e) => {
      e.preventDefault();
      exportExams();
    });
  }

  document.addEventListener('DOMContentLoaded', init);
  window.ExamManagerReinit = init;

  // ===== Filter =====
  function filterExams() {
    const status = examStatusFilter ? examStatusFilter.value.toLowerCase() : '';
    const search = examSearchInput ? examSearchInput.value.trim().toLowerCase() : '';

    qsa('.data-table tbody tr').forEach(tr => {
      const tds = tr.querySelectorAll('td');
      if (tds.length < 4) return;

      const tenKyThi  = tds[1].innerText.toLowerCase();
      const monHoc    = tds[2].innerText.toLowerCase();
      const ttText    = tds[3].innerText.toLowerCase();

      const matchText   = !search || tenKyThi.includes(search) || monHoc.includes(search);
      let   matchStatus = true;
      if (status) {
        if (status === 'running')  matchStatus = ttText.includes('đang diễn ra');
        if (status === 'upcoming') matchStatus = ttText.includes('sắp diễn ra');
        if (status === 'finished') matchStatus = ttText.includes('đã kết thúc');
      }
      tr.style.display = (matchText && matchStatus) ? '' : 'none';
    });
  }

  // ===== Export =====
  function exportExams() {
    const rows = qsa('.data-table tbody tr').filter(tr => tr.style.display !== 'none');
    const data = rows.map(tr => {
      const cells = tr.querySelectorAll('td');
      return {
        STT:       (cells[0]?.innerText || '').trim(),
        TenKyThi:  (cells[1]?.innerText || '').trim(),
        MonHoc:    (cells[2]?.innerText || '').trim(),
        TrangThai: (cells[3]?.innerText || '').trim()
      };
    }).filter(x => x.TenKyThi);

    if (!data.length) { alert('Không có dữ liệu để export!'); return; }

    if (typeof XLSX !== 'undefined' && XLSX.utils && XLSX.writeFile) {
      const ws = XLSX.utils.json_to_sheet(data);
      const wb = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Exams');
      XLSX.writeFile(wb, 'exams.xlsx');
      alert('Export Excel thành công!');
      return;
    }

    // Fallback CSV nếu chưa nhúng SheetJS
    const headers = Object.keys(data[0]);
    const csv = [
      headers.join(','), 
      ...data.map(row => headers.map(h => {
        const v = String(row[h] ?? '');
        return /[",\n]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v;
      }).join(','))
    ].join('\n');

    const blob = new Blob([csv], { type:'text/csv;charset=utf-8;' });
    const url  = URL.createObjectURL(blob);
    const a    = document.createElement('a');
    a.href = url; a.download = 'exams.csv';
    document.body.appendChild(a); a.click(); a.remove();
    URL.revokeObjectURL(url);
    alert('Export CSV thành công (chưa có SheetJS)!');
  }
  window.exportExams = exportExams;
})();
