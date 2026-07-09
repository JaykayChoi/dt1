// "유니티 프로젝트 열기" 버튼 — serve.py(open-docs.bat)로 볼 때만 동작한다.
// 버튼의 data-open 속성이 열 프로젝트를 지정한다 (예: data-open="fabsim").
// file:// 로 열었을 때는 안내 문구로 대체한다.
(function () {
  'use strict';

  function setNote(button, text, revertAfterMs) {
    var label = button.querySelector('.btn-label');
    if (!label) {
      return;
    }
    var original = label.getAttribute('data-original') || label.textContent;
    label.setAttribute('data-original', original);
    label.textContent = text;
    if (revertAfterMs) {
      setTimeout(function () {
        label.textContent = original;
        button.classList.remove('busy');
      }, revertAfterMs);
    }
  }

  document.addEventListener('click', function (e) {
    var button = e.target.closest && e.target.closest('.btn-launch');
    if (!button) {
      return;
    }
    e.preventDefault();

    var key = button.getAttribute('data-open') || 'fabsim';

    var host = location.hostname;
    var isLocalHost = host === 'localhost' || host === '127.0.0.1' || host === '[::1]';

    if (location.protocol === 'file:') {
      // file:// 로 열었을 때는 dt1open:// 프로토콜 핸들러로 에디터를 연다.
      // 브라우저가 처음 한 번 "열기 허용" 확인창을 띄운다.
      setNote(button, '브라우저 확인창이 뜨면 "열기"를 허용하세요', 8000);
      location.href = 'dt1open://' + key;
      return;
    }
    if (!isLocalHost) {
      // GitHub Pages 등 원격 호스팅에서는 로컬 에디터를 열 수 없다.
      // 이 사이트를 내려받아 open-docs.bat 으로 실행할 때만 동작한다.
      setNote(button, '이 버튼은 내 PC에서 학습할 때만 동작합니다 (저장소를 clone 후 open-docs.bat 실행)', 10000);
      return;
    }
    button.classList.add('busy');
    setNote(button, '에디터 실행 요청 중…');
    fetch('/open/' + key)
      .then(function (r) { return r.json(); })
      .then(function (res) {
        if (res.ok) {
          setNote(button, '에디터 여는 중 — 첫 실행은 몇 분 걸립니다', 8000);
        } else {
          setNote(button, '실패: ' + res.message, 8000);
        }
      })
      .catch(function () {
        setNote(button, '서버 응답 없음 — open-docs.bat으로 열었는지 확인', 8000);
      });
  });
})();
