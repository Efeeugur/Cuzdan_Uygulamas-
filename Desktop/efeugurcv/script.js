const terminal = document.getElementById('terminal');
let inputBuffer = '';
const promptSymbol = 'efeugur@portfolio:~$';

const commands = {
  help: `Komutlar:\nwhoami - Hakkımda\nedu - Eğitim\nskills - Beceriler\nprojects - Projeler\nlang - Diller\ncontact - İletişim\nclear - Temizle`,
  whoami: `Merhaba, ben Efe Uğur, Ankara Medipol Üniversitesi Bilgisayar Mühendisliği öğrencisiyim. Bilgisayar dünyasına olan ilgim ve merakım, beni bu dinamik ve yenilikçi alanda kendimi geliştirmeye yönlendirdi. Bilgisayar Mühendisliği eğitimimle birlikte, yazılım geliştirme, algoritma analizi ve problem çözme konularında güçlü bir temel oluşturuyorum.`,
  edu: `EĞİTİM:\n- Ankara Medipol Üniversitesi\n  Bilgisayar Mühendisliği (2021- )\n- Adnan Menderes Anadolu Lisesi (2017-2021)`,
  skills: `BECERİLER:\n- JAVA\n- HTML, CSS, JS\n- C\n\nSTAJ:\n- DEDAŞ Kurumsal Uygulamalar Müdürlüğü (2024)\n  2024 yılında Dicle Elektrik Dağıtım A.Ş.'de yaz stajı gerçekleştirdim. İş analistleriyle birlikte çalışarak QDMS, LEGA, EBYS, Ensemble ve Jira gibi sistemlerin test süreçlerinde görev aldım, hata tespitleri ve iyileştirme önerileri sundum.`,
  projects: `PROJELER:\n1. TEKNOFEST 2024\n   Sayısal Alanda Çip Tasarımı (UNIMEDA URUNGU)\n   Bellek, önbellek kontrolcüsü ve çevre birimi tasarımı\n2. Oyun Yarışması (California State Üniversitesi, 27 Şubat 2023)\n   Takım olarak 3.lük elde ettik.`,
  lang: `DİLLER:\n- İngilizce (B2 seviyesinde)\n\nSERTİFİKALAR:\n- Java101 Turkcell\n- Java201 Turkcell (x2)`,
  contact: `EFE UĞUR\nBİLGİSAYAR MÜHENDİSİ\n+90 545 931 98 01\nefeugur216@gmail.com\nMezopotamya Mahallesi\n608. Sokak Karakoç Golden Line Sitesi A Blok\nKat:1 No:7`,
};

function printLine(text = '') {
  const div = document.createElement('div');
  div.textContent = text;
  terminal.appendChild(div);
}

function printPrompt() {
  const div = document.createElement('div');
  div.innerHTML = `<span class="prompt">${promptSymbol} </span><span id="input"></span><span class="cursor">_</span>`;
  terminal.appendChild(div);
}

function clearTerminal() {
  terminal.innerHTML = '';
}

function handleCommand(cmd) {
  const trimmed = cmd.trim();
  if (trimmed === 'clear') {
    clearTerminal();
    return;
  }
  const output = commands[trimmed] || `Komut bulunamadı: ${trimmed}`;
  output.split('\n').forEach(printLine);
}

function commitCommand() {
  const cursor = document.querySelector('.cursor');
  if (cursor) cursor.remove();
  const inputSpan = document.getElementById('input');
  if (inputSpan) inputSpan.removeAttribute('id');
  handleCommand(inputBuffer);
  inputBuffer = '';
  printPrompt();
  scrollTo(0, document.body.scrollHeight);
}

document.addEventListener('keydown', (e) => {
  const inputSpan = document.getElementById('input');
  if (e.key === 'Backspace') {
    inputBuffer = inputBuffer.slice(0, -1);
  } else if (e.key === 'Enter') {
    commitCommand();
    return;
  } else if (e.key.length === 1) {
    inputBuffer += e.key;
  }
  if (inputSpan) inputSpan.textContent = inputBuffer;
});

const welcomeText = [
'   ______  ______  ______   _    _   _____  _    _  _____' ,
'  |  ____||  ____||  ____| | |  | | / ____|| |  | ||  __ \ ' ,
'  | |__   | |__   | |__    | |  | || |  __ | |  | || |__) | ' , 
'  |  __|  |  __|  |  __|   | |  | || | |_ || |  | ||  _  / ',
'  | |____ | |     | |____  | |__| || |__| || |__| || | \ \ ' ,
'  |______||_|     |______|  \____/  \_____| \____/ |_|  \_\ ' ,
  '',
  'v1.0.0',
  '',
  'EFE UĞUR\'un dijital terminal portfolyosuna hoş geldiniz!',
  'Komutları görmek için `help` yazın.'
];

welcomeText.forEach(printLine);
printPrompt();