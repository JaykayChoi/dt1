// file:// 프로토콜로 열면 YouTube 임베드가 리퍼러 부재로 오류 153을 내므로,
// 썸네일 + 새 탭 링크로 자동 전환한다. (http/https로 열면 임베드 그대로 재생)
(function () {
  if (location.protocol !== 'file:') return;
  document.querySelectorAll('figure.media .frame iframe').forEach(function (iframe) {
    var m = (iframe.getAttribute('src') || '').match(/embed\/([\w-]{11})/);
    if (!m) return;
    var id = m[1];
    var a = document.createElement('a');
    a.href = 'https://www.youtube.com/watch?v=' + id;
    a.target = '_blank';
    a.rel = 'noopener';
    a.className = 'yt-fallback';
    a.innerHTML =
      '<img src="https://i.ytimg.com/vi/' + id + '/hqdefault.jpg" alt="YouTube 영상 썸네일">' +
      '<span class="play">▶ YouTube에서 열기</span>';
    iframe.replaceWith(a);
  });
})();
