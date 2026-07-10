// 용어 툴팁 시스템 — 본문에서 용어의 첫 등장(섹션 단위)에 밑줄을 긋고,
// 호버/탭하면 정의와 어원을 툴팁으로 보여준다. 사전은 이 파일 하나로 관리한다.
(function () {
  'use strict';

  // ---------- 용어 사전 ----------
  var D = {};
  function T(names, en, def, ety) {
    var e = { name: names[0], en: en, def: def, ety: ety || null };
    names.forEach(function (n) { D[n] = e; });
  }

  // 반송 장비
  T(['OHT'], 'Overhead Hoist Transport',
    '천장 레일을 달리는 무인 비히클이 FOUP을 싣고 이동하다, 목적지 장비 위에서 호이스트로 내려놓는 반도체 팹의 주력 반송 시스템.',
    'over(위)+head(머리)로 "머리 위의", hoist는 "끌어올리다", transport는 라틴어 trans(가로질러)+portare(나르다). 합치면 "머리 위에서 들어올려 실어 나르는 것" — 이름이 곧 동작 설명이다.');
  T(['AGV'], 'Automated Guided Vehicle',
    '바닥의 가이드 인프라(자기 테이프·QR·반사판)를 따라 정해진 경로만 주행하는 무인 운반차.',
    '핵심은 guided(유도되는) — "스스로 가는 차"가 아니라 "이끌려 가는 차"라는 이름 자체가 경로 자유도의 한계를 말해 준다. vehicle은 라틴어 vehere(나르다)에서 온 vehiculum(운반 수단).');
  T(['AMR'], 'Autonomous Mobile Robot',
    'SLAM으로 스스로 지도를 만들고 위치를 추정하며, 경로를 동적으로 계산해 주행하는 자율 이동 로봇.',
    'autonomous는 그리스어 autos(자신)+nomos(법) — "스스로에게 법을 주는". robot은 체코어 robota(고된 노동)에서 왔고, 카렐 차페크의 1920년 희곡 R.U.R.에서 처음 쓰였다.');
  T(['SLAM'], 'Simultaneous Localization and Mapping',
    '지도를 만들면서 동시에 그 지도 안에서 자기 위치를 추정하는 기법. AMR 자율주행의 핵심 기술.',
    'simultaneous는 라틴어 simul(동시에)에서 — "지도 작성"과 "위치 추정"이라는 닭과 달걀 문제를 동시에 푼다는 뜻이 이름에 그대로 들어 있다.');
  T(['비히클'], 'vehicle',
    'OHT 레일 위를 달리는 무인 대차 한 대. 현장에서 "OHT 한 대"를 가리키는 말.',
    '라틴어 vehere(나르다)에서 온 vehiculum(운반 수단)이 어원이다.');
  T(['호이스트'], 'hoist',
    '비히클이 레일에 정지한 채 FOUP을 수직으로 내리고 올리는 승강 장치, 또는 그 동작.',
    '네덜란드어 hijsen(끌어올리다)에서 온 뱃사람들의 말 — 원래 돛을 끌어올릴 때 쓰던 단어다.');

  // 팹 구조와 용기
  T(['팹'], 'fab',
    '반도체 전공정 생산 공장.',
    'fabrication (plant)의 준말. 라틴어 faber(장인), fabricare(만들다)에서 왔다 — 말 그대로 "만드는 곳".');
  T(['서브팹'], 'subfab',
    '클린룸 바로 아래층. 펌프·가스·배관 등 지원 설비가 들어가며, 레일이 닿지 않아 AGV/AMR가 활약하는 공간.',
    'sub(아래)+fab — "팹 아래층"이라는 뜻 그대로다.');
  T(['클린룸'], 'cleanroom',
    '공기 중 미세 입자 수를 극도로 통제한 생산 공간. 사람이 최대 오염원이라 반송을 자동화한다.',
    '말 그대로 "깨끗한 방". 1960년대 미국의 군수·우주 부품 조립에서 시작된 개념이 반도체 생산의 상징이 됐다.');
  T(['베이'], 'bay',
    '비슷한 공정 장비들이 통로 양쪽으로 도열한 팹의 구획 단위.',
    '고대 프랑스어 baee(열린 곳·틈)에서 왔다. 건축에서 기둥과 기둥 사이 "한 칸"을 bay라 부르는데, 그 한 칸이 공장의 구획으로 이어졌다.');
  T(['인터베이'], 'interbay',
    '베이와 베이 사이를 잇는 간선 반송.',
    '라틴어 inter(~사이) — international(나라 사이)과 같은 접두사다.');
  T(['인트라베이'], 'intrabay',
    '한 베이 안, 장비와 장비 사이의 반송.',
    '라틴어 intra(~안쪽) — intranet(내부망)과 같은 접두사. inter(사이)와 한 글자 차이로 안팎이 갈린다.');
  T(['FOUP'], 'Front Opening Unified Pod',
    '300mm 웨이퍼를 최대 25매 담는 표준 밀폐 운반 용기. 반송 시스템이 실제로 집고 나르는 최소 단위.',
    '"앞면이(front) 열리는(opening), 규격이 통일된(unified) 포드(pod)". pod는 본래 콩깍지 — 내용물을 감싸 보호하는 껍질이라는 은유다.');
  T(['웨이퍼'], 'wafer',
    '지름 300mm의 얇은 실리콘 원판. 이 위에 회로가 새겨진다.',
    '얇게 구운 과자(웨하스)를 뜻하던 말로, waffle(와플)과 같은 뿌리다. 얇고 둥근 모양에서 왔다.');
  T(['로트'], 'lot',
    '같은 공정 이력을 공유하며 함께 움직이는 웨이퍼 묶음. 생산 관리와 추적의 기본 단위.',
    '고대 영어 hlot(제비뽑기에 쓰던 나뭇조각)에서 "제비로 나눈 몫"이 되었고, 다시 "한 묶음의 물량"으로 의미가 넓어졌다.');
  T(['레티클'], 'reticle',
    '노광 공정에서 회로 패턴의 원판이 되는 포토마스크. 전용 밀폐 포드(레티클 포드)에 담겨 반송된다.',
    '라틴어 reticulum(작은 그물)에서 왔다. 망원경의 조준 격자처럼 가는 선이 새겨진 광학 부품을 부르던 말이 반도체 마스크로 이어졌다.');
  T(['캐리어'], 'carrier',
    'FOUP·레티클 포드처럼 운반물을 담아 나르는 용기의 총칭.',
    'carry는 라틴어 carrus(수레)에서 왔다 — "수레에 싣다"가 "나르다"가 됐다.');
  T(['로드포트'], 'load port',
    '장비 전면에서 FOUP을 받아 도킹하고 도어를 열어 주는 접점. 반송의 최종 목적지.',
    'port는 라틴어 porta(문) — 화물이 드나드는 "문"이라는 뜻 그대로다. 항구(port)도 같은 뿌리.');
  T(['스토커'], 'stocker',
    'FOUP을 대량 보관하는 자동 창고. 다층 선반과 내부 크레인, 입출고 포트로 구성된다.',
    'stock(재고·저장품)에서 왔다. stock은 본래 나무 그루터기·기둥을 뜻하다가 "쌓아 둔 물자"로 확장됐다 — "재고를 쌓아 두는 곳"이라는 직설적 작명.');
  T(['사이드 트랙 버퍼', 'STB', '버퍼'], 'Side Track Buffer',
    '레일 옆·아래에 붙은 소형 선반. 비히클이 호이스트로 직접 얹고 집을 수 있는 근거리 완충 장소.',
    'buffer는 "치다"를 뜻하던 buff에서 온 "충격을 죽이는 것" — 완충기다. 물류에서는 흐름의 출렁임을 흡수하는 저장 공간을 가리킨다.');
  T(['AMHS'], 'Automated Material Handling System',
    '차량·레일·스토커·버퍼·제어 소프트웨어까지, 팹의 자동 반송에 관련된 모든 것의 총칭.',
    'material handling(자재 취급)은 물류공학의 고전 용어로, "물건을 옮기고, 보관하고, 건네는 일" 전체를 묶는 말이다.');
  T(['반송'], '搬送 · transport',
    '자재를 한 지점에서 다른 지점으로 옮기는 일. 이 도메인의 중심 개념.',
    '한자 搬送 — 搬(옮길 반)+送(보낼 송), "옮겨 보냄". 영어로는 transport 또는 material handling에 해당한다.');

  // 제어 시스템
  T(['MES'], 'Manufacturing Execution System',
    '로트 상태를 추적하고 다음 공정과 장비를 결정하는 생산 실행 시스템. 제어 계층의 꼭대기.',
    'manufacture는 라틴어 manu(손으로)+facere(만들다) — "손으로 만들기"라는 말이 대량 생산의 이름이 된 셈이다.');
  T(['MCS'], 'Material Control System',
    'MES의 반송 요구를 반송 명령(job)으로 바꿔 AMHS 전체를 지휘하는 관제탑. 캐리어 위치의 단일 진실.',
    'control은 중세 라틴어 contrarotulus(대조 장부)에서 왔다 — 본래 "장부로 대조·검증하다"라는 뜻이 "통제하다"로 넓어졌다.');
  T(['OCS'], 'OHT Control System',
    'OHT 비히클 함대의 배차·경로·교통 제어를 맡는 차량 관제 계층. 제조사마다 명칭이 다르다.',
    null);
  T(['배차'], 'dispatching',
    '새 반송 명령을 어느 차량에 맡길지 정하는 결정. 최근접 유휴 차량 규칙이 대표적.',
    'dispatch는 이탈리아어 dispacciare(서둘러 처리하다)에서 — "신속히 내보냄". 한자 配車는 配(나눌 배)+車(수레 차), "차를 나누어 배정함".');
  T(['라우팅', '경로 계획'], 'routing',
    '출발지에서 목적지까지 어느 길로 갈지 정하는 경로 계획. 혼잡을 반영해야 실전이 된다.',
    'route는 라틴어 rupta via("부수어 뚫은 길", rumpere=부수다)에서 왔다 — 숲을 뚫어 낸 길이 route가 됐다.');
  T(['교통 제어'], 'traffic control',
    '구간 점유, 합류 중재, 간격 유지로 차량 흐름을 안전하게 관리하는 일.',
    'traffic은 이탈리아어 traffico(왕래·거래)에서 — 상인과 물자의 "오감"이 차량의 "오감"이 됐다.');
  T(['데드락'], 'deadlock',
    '차량(또는 프로세스)들이 서로가 점유한 자원을 서로 기다리며, 외부 개입 없이는 영원히 멈추는 상태.',
    'dead(죽은)+lock(잠금) — "꿈쩍도 하지 않는 잠금". 번역어 교착(膠着)은 "아교로 붙은 듯 달라붙음"이라는 뜻이다.');
  T(['스테이징'], 'staging',
    '수요가 예상되는 위치에 유휴 차량이나 화물을 미리 대기시키는 것.',
    'stage는 라틴어 stare(서다)에서 — "세워 두는 자리". 연극 무대(stage)와 같은 뿌리다.');
  T(['인터록'], 'interlock',
    '조건이 갖춰지기 전에는 동작을 막는 안전장치. E84 핸드셰이크가 반송 인터록의 예.',
    'inter(서로)+lock(잠금) — "서로 맞물린 잠금". 한쪽이 풀리지 않으면 다른 쪽이 움직일 수 없다.');
  T(['SEMI'], 'Semiconductor Equipment and Materials International',
    '반도체 장비·재료 업계의 국제 협회. 장비 상호운용 표준(E 시리즈)을 만든다.',
    null);
  T(['SECS/GEM', 'SECS', 'GEM'], 'SECS(통신 규약) / GEM(공통 장비 모델)',
    '팹 장비와 호스트가 메시지를 주고받는 표준 프로토콜(SECS)과, 장비 공통의 동작 모델(GEM).',
    'GEM의 generic은 라틴어 genus(종류)에서 — "장비라면 종류 불문 공통으로"라는 뜻이 이름에 들어 있다.');
  T(['SEM'], 'Specific Equipment Model',
    'GEM 공통 모델 위에 특정 장비 유형(스토커, 반송 차량 등)의 상태·명령을 추가한 표준 모델.',
    'specific은 라틴어 species(종)에서 — generic(공통)과 대비되는 "종 특이적" 모델이라는 뜻.');
  T(['디지털 트윈'], 'digital twin',
    '실물 시스템의 상태를 실시간으로 반영하는 디지털 복제본.',
    'twin은 고대 영어 twinn(둘씩의, twi-=둘)에서 — "실물과 쌍둥이"라는 은유다.');

  // 물류 지표 · 시뮬레이션
  T(['처리량', '스루풋'], 'throughput',
    '단위 시간당 완료한 작업(반송) 건수. 시스템 성능의 최상위 지표.',
    'through(통과)+put(놓다) — "시스템을 통과시켜 낸 양".');
  T(['가동률'], 'utilization',
    '자원이 실제로 일한 시간의 비율. 너무 높으면 대기 시간이 비선형으로 폭증한다.',
    '라틴어 uti(쓰다)에서 온 utilize — "쓸모 있게 씀". 한자 稼動은 "기계를 움직여 일하게 함"이라는 뜻.');
  T(['공차 주행', '빈 주행'], 'empty travel · deadheading',
    '짐 없이 이동하는 주행. 배차 품질이 나쁠수록 늘어나는 대표적 낭비.',
    '空車(빌 공+수레 차). 영어 deadheading은 철도에서 "수익 없이(dead) 이동하는 차량"을 부르던 말이다.');
  T(['병목'], 'bottleneck',
    '전체 처리량을 제한하는 가장 느린 구간·자원. 최적화의 1순위 대상.',
    '병(bottle)의 목(neck) — 병을 기울여도 목의 굵기만큼만 흘러나온다는 비유 그대로다.');
  T(['이산사건시뮬레이션', 'DES'], 'Discrete Event Simulation',
    '상태가 변하는 "사건"의 시점으로만 시간을 건너뛰며 계산하는 시뮬레이션 기법. 물류 모델링의 핵심.',
    'discrete는 라틴어 discretus(분리된), event는 evenire(밖으로 나오다·일어나다), simulation은 similis(비슷한)에서 — "띄엄띄엄 일어나는 사건들로 현실을 흉내 내기".');
  T(['이벤트 큐', 'FEL', '대기열'], 'queue · Future Event List',
    '앞으로 일어날 사건을 시각순으로 정렬해 둔 우선순위 큐(FEL), 또는 자원을 기다리는 줄.',
    'queue는 라틴어 cauda(꼬리)에서 — 프랑스어에서 "땋은 머리"였다가 영어에서 "기다리는 줄"이 됐다. 꼬리처럼 늘어선 줄.');
  T(['리소스'], 'resource',
    '장비·반송차·레일 구간처럼 동시에 쓸 수 있는 수가 제한된 자원. 점유와 대기가 여기서 생긴다.',
    '라틴어 resurgere(다시 솟아나다)에서 온 프랑스어 ressource — "다시 일어설 수단"이 "쓸 수 있는 자원"이 됐다.');
  T(['우선순위 큐'], 'priority queue',
    '넣는 순서와 무관하게 "가장 작은(이른) 것"부터 꺼내는 자료구조. DES에서는 미래 사건 목록(FEL)의 구현체.',
    'priority는 라틴어 prior(앞선)에서 — "앞선 것부터"라는 뜻이 이름에 그대로 있다. 보통 이진 힙(heap)으로 구현한다.');
  T(['도착률'], 'arrival rate · λ',
    '단위 시간당 도착하는 요청(로트) 수. 관례적으로 λ(람다)로 쓴다.',
    null);
  T(['서비스율'], 'service rate · μ',
    '단위 시간당 처리할 수 있는 요청 수. 관례적으로 μ(뮤)로 쓴다. λ/μ가 가동률 ρ다.',
    null);
  T(['지수분포'], 'exponential distribution',
    '"다음 사건까지 얼마나 걸리나"를 모델링하는 분포. 과거를 기억하지 않는(무기억) 성질이 핵심.',
    'exponent(지수)는 라틴어 exponere(밖에 내놓다)에서 — 확률밀도가 지수함수 꼴로 감소해서 붙은 이름이다.');
  T(['포아송 과정', '포아송'], 'Poisson process',
    '사건이 서로 독립적으로, 일정한 평균 속도로 무작위하게 일어나는 도착 모델. 사건 간 간격이 지수분포를 따른다.',
    '프랑스 수학자 시메옹 드니 푸아송(Siméon Denis Poisson)의 이름에서 왔다. 참고로 poisson은 프랑스어로 "물고기"다.');
  T(['리틀의 법칙'], "Little's law",
    'L = λW — 시스템 안의 평균 개체 수(L)는 도착률(λ) × 평균 체류시간(W). 분포와 무관하게 성립하는 보편 법칙.',
    'MIT의 존 리틀(John Little)이 1961년 일반적인 증명을 내놓아 그의 이름이 붙었다.');
  T(['M/M/1'], 'M/M/1 queue',
    '포아송 도착(M) + 지수 서비스(M) + 서버 1대인 가장 단순한 대기행렬 모델.',
    '켄들 표기법(A/S/c) — 도착/서비스/서버 수. M은 Markovian(무기억)의 머리글자로, 수학자 안드레이 마르코프에서 왔다.');
  T(['코루틴'], 'coroutine',
    '중간에 실행을 멈췄다가(yield) 그 지점부터 재개할 수 있는 함수. SimPy는 이것으로 "시간이 흐르는 지점"을 표현한다.',
    'co-(함께)+routine(정해진 절차) — 서로 제어권을 양보하며 "함께 도는" 루틴이라는 뜻. routine은 route(길)에서 왔다.');
  T(['콜백'], 'callback',
    '"이 일이 끝나면 이 함수를 불러 달라"고 맡겨 두는 함수. DesCore 엔진이 흐름을 잇는 방식.',
    'call(부르다)+back(되돌려) — "나중에 다시 불러 준다"는 전화 용어에서 왔다.');

  // Unity 렌더링 · 라이팅
  T(['셰이더'], 'shader',
    '표면의 색과 음영을 픽셀 단위로 계산하는 GPU 프로그램. 머티리얼은 셰이더에 넘길 값들의 묶음이다.',
    'shade(그늘·음영)에서 — 원래 "음영을 넣는 것"이라는 뜻 그대로다.');
  T(['머티리얼'], 'material',
    '표면의 시각 속성(색, 금속성, 거칠기, 요철 등)을 셰이더 파라미터로 정의한 재질 에셋.',
    '라틴어 materia(재료, 원래는 목재) — "무엇으로 만들어졌는가"라는 뜻이 그대로 이름이 됐다.');
  T(['알베도'], 'albedo',
    '조명 정보가 전혀 없는 표면의 순수한 색. PBR의 Base Map이 담아야 하는 것.',
    '라틴어 albus(흰)에서 — 천문학에서 행성이 빛을 반사하는 비율을 부르던 말이다.');
  T(['노멀 맵'], 'normal map',
    '픽셀마다 표면의 기울기(법선)를 저장해, 지오메트리 없이 요철의 음영을 만들어 내는 텍스처.',
    'normal(법선)은 라틴어 norma(직각자)에서 — 목수의 직각자가 수학의 "수직인 선"이 됐다.');
  T(['프레넬'], 'Fresnel',
    '시선이 표면과 스칠수록(가장자리) 반사가 강해지는 현상. 모든 재질이 갖는 물리 성질로, PBR 셰이더에 내장되어 있다.',
    '프랑스 물리학자 오귀스탱 장 프레넬(Augustin-Jean Fresnel)의 이름에서 왔다. 등대 렌즈의 그 프레넬이다.');
  T(['스무스니스'], 'smoothness',
    '표면의 매끄러움. 높을수록 반사가 선명해지고 낮을수록 뿌옇게 퍼진다. roughness(거칠기)의 반대 표현.',
    null);
  T(['라이트맵'], 'lightmap',
    '정적 물체가 받는 빛(특히 간접광)을 미리 계산해 구워 둔 텍스처. 런타임에는 그냥 읽기만 한다.',
    null);
  T(['베이킹', '베이크'], 'baking',
    '비싼 조명 계산을 미리 수행해 텍스처(라이트맵)나 프로브에 저장해 두는 것.',
    '"굽는다"는 요리 은유 — 재료(씬과 조명)를 오븐(라이트매퍼)에 넣고 기다리면 완성품(라이트맵)이 나온다. 다시 굽기 전엔 재료를 바꿔도 맛이 안 변한다는 것까지 은유가 통한다.');
  T(['전역 조명', 'GI'], 'global illumination',
    '광원에서 직접 오는 빛만이 아니라, 표면에 튕겨 돌아다니는 간접광까지 포함한 조명 계산.',
    'global(전체의)+illumination(조명) — 빛이 "장면 전체를 돌아다닌다"는 뜻. 라틴어 illuminare(빛을 비추다)에서.');
  T(['라이트 프로브'], 'light probe',
    '공간 곳곳의 간접광을 미리 담아 두는 측정점. 움직이는 물체가 구운 조명과 어울리게 만드는 장치.',
    'probe는 라틴어 probare(시험하다·검사하다)에서 — "찔러 보는 탐침"이라는 뜻이 측정 장치의 이름이 됐다.');
  T(['리플렉션 프로브'], 'reflection probe',
    '특정 위치에서 주변을 큐브맵으로 캡처해, 금속·유리 표면의 반사에 쓰는 장치.',
    null);
  T(['이미시브'], 'emissive',
    '스스로 빛을 내는 표면 속성. 장비 상태등, 모니터 화면 등에 쓰고, 블룸과 만나면 발광이 강조된다.',
    '라틴어 emittere(밖으로 내보내다)에서 — "빛을 내보내는" 표면.');
  T(['톤매핑'], 'tone mapping',
    'HDR로 계산된 밝기 범위를 모니터가 표시할 수 있는 범위로 눌러 담는 변환. ACES가 사실상 표준.',
    'tone(색조)은 그리스어 tonos(팽팽함, 현의 장력)에서 — 음악의 톤과 같은 뿌리다.');
  T(['ACES'], 'Academy Color Encoding System',
    '영화 업계 표준 색 인코딩·톤매핑 시스템. Unity에서 켜면 "필름 룩"의 대비와 색 반응을 얻는다.',
    '오스카상을 주는 그 영화예술과학아카데미(Academy of Motion Picture Arts and Sciences)가 만든 표준이라 이 이름이다.');
  T(['블룸'], 'bloom',
    '밝은 부분이 주변으로 번져 빛나 보이게 하는 포스트 효과. HDR 값이 있어야 자연스럽다.',
    '"꽃이 핀다"는 뜻의 bloom — 빛이 꽃처럼 피어나듯 번진다는 은유다.');
  T(['비네트'], 'vignette',
    '화면 가장자리를 어둡게 눌러 시선을 중앙으로 모으는 포스트 효과.',
    '프랑스어로 "작은 덩굴" — 책 페이지 가장자리를 덩굴 문양으로 장식하던 데서, "가장자리 처리"라는 뜻이 됐다.');
  T(['앰비언트 오클루전', 'SSAO', 'AO'], 'ambient occlusion',
    '구석·틈새처럼 주변광이 도달하기 어려운 곳을 어둡게 만들어 입체감을 살리는 효과. SSAO는 화면 공간에서 실시간 계산하는 방식.',
    'occlusion은 라틴어 occludere(막다)에서 — "주변광(ambient)이 막힌 정도"라는 뜻 그대로다.');
  T(['HDR'], 'High Dynamic Range',
    '모니터가 표시할 수 있는 범위를 넘는 밝기까지 그대로 계산·저장하는 방식. 블룸·톤매핑의 전제 조건.',
    null);
  T(['URP'], 'Universal Render Pipeline',
    '성능과 확장성의 균형을 맞춘 Unity의 범용 렌더 파이프라인. 이 프로젝트(FabSim)의 렌더링 기반.',
    null);

  // Unity 그래픽스 심화 — 파이프라인 · 셰이더 · 시각화
  T(['SRP'], 'Scriptable Render Pipeline',
    '렌더링 절차를 C# 코드로 정의할 수 있게 연 Unity의 구조. URP와 HDRP가 이 위에 만들어진 파이프라인이다.',
    'scriptable(스크립트로 제어 가능한)+render(그리다)+pipeline(관) — 본래 블랙박스였던 렌더 과정을 "스크립트로 조립하는 관"으로 연다는 뜻.');
  T(['HDRP'], 'High Definition Render Pipeline',
    '고사양 PC·콘솔을 겨냥한 고충실도 렌더 파이프라인. 포토리얼 품질이 목표일 때 URP 대신 택한다.',
    'high definition(고선명)+render pipeline — "고선명 렌더 관"이라는 이름 그대로다.');
  T(['렌더러 피처', 'Renderer Feature'], 'Renderer Feature',
    'URP의 Renderer에 렌더 패스를 덧꽂는 플러그인. SSAO·데칼·외곽선이 모두 이 방식으로 붙는다.',
    'feature는 라틴어 factura(만듦새)에서 — "렌더러에 덧대는 기능 조각"이라는 뜻.');
  T(['렌더 패스'], 'render pass',
    '한 프레임을 그리는 과정 중 하나의 단계(그림자·불투명·투명·후처리 등). Renderer는 이 패스들의 순서다.',
    'pass는 "한 번 훑고 지나감" — 화면을 완성하려 데이터를 여러 번 "지나가며" 처리하는 각 회차.');
  T(['포워드'], 'forward rendering',
    '오브젝트를 그릴 때마다 그 표면에 닿는 라이트를 곧바로 계산하는 방식. 라이트가 적을 때 유리하고 MSAA와 잘 맞는다.',
    'forward(앞으로) — 지오메트리에서 화면으로 "곧장 앞으로" 밀고 나가며 조명을 계산한다는 뜻.');
  T(['디퍼드'], 'deferred rendering',
    '표면 정보(색·노멀·깊이)를 버퍼에 먼저 모아 두고, 조명 계산을 화면 공간에서 나중에 몰아 하는 방식. 라이트가 많을 때 유리.',
    'defer는 라틴어 differre(뒤로 미루다)에서 — 조명 계산을 "뒤로 미룬다"는 이름 그대로다.');
  T(['감마', 'sRGB'], 'gamma · sRGB',
    '사람 눈과 모니터에 맞춰 밝기를 비선형으로 인코딩한 색 공간. 이미지는 이 공간에 저장되지만 빛의 물리 계산은 선형 공간에서 해야 옳다.',
    '그리스 문자 γ(감마) — 밝기 변환 곡선의 지수를 관례적으로 γ로 표기한 데서 붙은 이름이다.');
  T(['선형'], 'linear color space',
    '빛의 세기에 색 값이 정비례하는 색 공간. 더하고 곱하는 조명 계산이 물리적으로 맞아떨어지는 유일한 공간이라 PBR의 전제가 된다.',
    'linear는 라틴어 linea(선) — 입력과 출력이 "직선" 관계라는 뜻.');
  T(['에일리어싱', '계단현상'], 'aliasing',
    '연속적인 형태를 픽셀 격자로 표본화할 때 비스듬한 엣지가 계단처럼 어긋나 보이는 현상.',
    'alias는 라틴어 alias(다른 이름으로) — 신호처리에서 높은 주파수가 "다른(가짜) 주파수로 둔갑"해 나타나는 데서 왔다.');
  T(['MSAA'], 'Multisample Anti-Aliasing',
    '지오메트리 엣지에서 픽셀당 여러 지점을 표본화해 계단을 부드럽게 하는 하드웨어 안티에일리어싱. 포워드 렌더링에서 상대적으로 저렴하다.',
    'multi(여럿)+sample(표본) — 픽셀 하나를 "여러 번 표본"해 평균 낸다는 뜻.');
  T(['FXAA'], 'Fast Approximate Anti-Aliasing',
    '완성된 화면에서 엣지를 찾아 흐리게 다듬는 후처리 방식. 가장 싸지만 디테일이 약간 뭉개진다.',
    'fast(빠른)+approximate(근사) — "빠르고 근사적인" 계단 제거라는 이름.');
  T(['SMAA'], 'Subpixel Morphological Anti-Aliasing',
    '엣지의 형태를 분석해 FXAA보다 선명하게 다듬는 후처리 안티에일리어싱.',
    'morphological은 그리스어 morphe(형태) — 엣지의 "형태를 분석해" 재구성한다는 뜻.');
  T(['TAA'], 'Temporal Anti-Aliasing',
    '여러 프레임에 걸쳐 화면을 미세하게 흔들어 표본을 시간축으로 누적하는 방식. 품질이 가장 높지만 움직임에서 잔상(고스팅)이 생길 수 있다.',
    'temporal은 라틴어 tempus(시간) — 여러 "시간"(프레임)의 표본을 모은다는 뜻.');
  T(['셰이더 그래프', 'Shader Graph'], 'Shader Graph',
    '노드를 연결해 셰이더를 만드는 Unity의 비주얼 에디터. 코드 대신 그래프로 표면의 색·발광·투명을 조립한다.',
    'graph는 그리스어 graphein(그리다·쓰다) — 노드와 선으로 "그린" 셰이더라는 뜻.');
  T(['마스터 스택'], 'Master Stack',
    'Shader Graph의 최종 출력 지점. Vertex·Fragment 블록에 Base Color·Emission·Alpha 등을 연결하면 셰이더가 완성된다.',
    'master(최종)+stack(쌓은 더미) — 그래프의 결과가 모이는 "최종 출력 더미".');
  T(['블랙보드 프로퍼티', '블랙보드'], 'Blackboard property',
    'Shader Graph에서 머티리얼 인스펙터에 노출되는 입력 값(색·수치 등). 스크립트의 SetColor/SetFloat로 런타임에 바꿔 상태를 표현한다.',
    'blackboard(칠판) — 그래프가 참조할 값들을 "칠판에 적어 두는" 목록이라는 은유.');
  T(['렌더 큐'], 'render queue',
    '오브젝트를 그리는 순서를 정하는 번호. 불투명은 앞에서, 투명은 뒤에서 그려야 겹침이 올바르게 보인다.',
    'queue(줄)는 라틴어 cauda(꼬리)에서 — 그리기 순서를 기다리는 "줄"이라는 뜻.');
  T(['엑스레이', 'X-ray'], 'X-ray',
    '표면을 반투명하게 만들어 케이스 내부나 벽 뒤 물체를 비쳐 보이게 하는 시각화. 프레넬·깊이로 만든다.',
    '1895년 뢴트겐이 정체를 몰라 미지수 "X"를 붙인 방사선(X-ray)에서 — "속을 꿰뚫어 본다"는 뜻으로 넓어졌다.');
  T(['라인 렌더러', 'LineRenderer'], 'LineRenderer',
    '지정한 점들을 이어 3D 공간에 선을 그리는 컴포넌트. OHT 레일·경로 그래프를 시각화하는 데 쓴다.',
    'line(선)+render(그리다) — 점들을 이어 "선을 그리는 것".');
  T(['트레일 렌더러', 'TrailRenderer'], 'TrailRenderer',
    '움직이는 물체 뒤에 최근 경로를 잔상으로 남기는 컴포넌트. 반송차의 이동 궤적을 보여 준다.',
    'trail(자취·꼬리) — 지나간 "자취"를 남긴다는 뜻.');
  T(['데칼'], 'decal',
    '지오메트리를 바꾸지 않고 표면에 이미지를 투영해 붙이는 기법. 바닥의 존 경계·정지선·경고 마킹에 쓴다.',
    '프랑스어 décalcomanie(전사 기법)의 준말 — 그림을 다른 표면에 "옮겨 붙인다"는 뜻.');
  T(['빌보드'], 'billboard',
    '항상 카메라를 향하도록 회전하는 평면. 장비 위 ID·상태 라벨이 어느 각도에서도 읽히게 한다.',
    'billboard(광고판) — 언제나 보는 사람을 향해 세워 둔 "광고판"에서 온 이름.');
  T(['직교 투영', '직교'], 'orthographic projection',
    '원근 왜곡 없이 평행하게 투영하는 카메라 방식. 지도처럼 거리·정렬이 그대로 보여 물류 관제 탑다운 뷰에 적합하다.',
    'ortho(직각)+graphein(그리다) — 화면에 "직각으로 곧게" 투영한다는 뜻. 그리스어 orthos(바른).');
  T(['원근 투영', '원근'], 'perspective projection',
    '먼 것이 작게 보이는, 사람 눈과 같은 투영. 현장감 있는 워크스루 뷰에 쓴다.',
    'perspective는 라틴어 perspicere(꿰뚫어 보다) — "멀고 가까움을 꿰뚫어 보는" 방식.');
  T(['카메라 스택', 'Camera Stack'], 'Camera Stack',
    'URP에서 Base 카메라 위에 Overlay 카메라를 겹쳐 UI·라벨·오버레이를 분리해 렌더하는 구성.',
    'stack(쌓기) — 여러 카메라의 결과를 "층층이 쌓아" 한 화면으로 합친다는 뜻.');
  T(['레이캐스트'], 'raycast',
    '한 점에서 광선을 쏴 처음 부딪히는 물체를 알아내는 기법. 화면 클릭으로 장비를 선택하는 상호작용의 기반.',
    'ray(광선)+cast(던지다) — 광선을 "던져" 무엇에 맞는지 본다는 뜻.');
  T(['월드 스페이스'], 'world space',
    '화면이 아니라 3D 씬의 실제 좌표 공간. 라벨을 이 공간에 두면 장비와 함께 공간에 붙어 움직인다.',
    'world(세계)+space(공간) — 화면(스크린) 좌표와 대비되는 "씬 세계의 좌표".');
  T(['TextMeshPro', 'TMP'], 'TextMeshPro',
    '외곽선·그림자·선명도가 뛰어난 Unity의 텍스트 렌더링 시스템. 월드 공간 라벨에 쓴다.',
    'text(글자)+mesh(그물망 지오메트리)+pro — 글자를 메시로 정교하게 그린다는 뜻.');

  // Unity 성능 · 최적화
  T(['드로우콜'], 'draw call',
    'CPU가 GPU에 "이것을 그려라"라고 보내는 명령 한 번. 수가 많을수록 CPU 쪽 준비 비용이 커진다.',
    'draw(그리다)+call(호출) — "그리기 호출"이라는 뜻 그대로다.');
  T(['배칭'], 'batching',
    '여러 오브젝트를 묶어 적은 드로우콜로 그리는 최적화의 총칭. SRP Batcher, GPU 인스턴싱, 정적 배칭이 대표.',
    'batch는 고대 영어 bacan(굽다)에서 — 빵을 "한 판에 같이 굽는 묶음"이 batch다. 라이트맵 베이킹과 같은 어원.');
  T(['GPU 인스턴싱'], 'GPU instancing',
    '같은 메시+머티리얼의 복제물 다수를 드로우콜 한 번으로 그리는 기법. 동일 장비 수백 대에 이상적.',
    'instance는 라틴어 instantia(존재하는 것, 사례)에서 — "같은 원형의 사례들"을 한 번에 그린다는 뜻.');
  T(['SRP 배처', 'SRP Batcher'], 'SRP Batcher',
    '같은 셰이더 변형을 쓰는 오브젝트들의 렌더 준비 비용을 묶어 주는 URP 기본 배칭. 머티리얼이 달라도 동작한다.',
    null);
  T(['LOD'], 'Level of Detail',
    '카메라와의 거리에 따라 단계적으로 단순한 메시로 바꿔 그리는 기법. 멀리 있는 것에 폴리곤을 낭비하지 않는다.',
    null);
  T(['오클루전 컬링'], 'occlusion culling',
    '다른 물체에 완전히 가려 보이지 않는 오브젝트를 아예 그리지 않는 기법.',
    'occlusion은 라틴어 occludere(막다), culling은 "솎아내기" — 가려진 것을 솎아낸다. 도축·선별에서 온 단어다.');
  T(['오브젝트 풀링'], 'object pooling',
    '자주 생성·파괴되는 오브젝트를 미리 만들어 두고 재사용하는 패턴. 생성 비용과 GC를 없앤다.',
    'pool은 "공동 웅덩이" — 여럿이 함께 쓰는 저수지에서 필요할 때 꺼내 쓰고 되돌려 놓는다는 은유.');
  T(['가비지 컬렉션', 'GC Alloc', 'GC'], 'garbage collection',
    '더 이상 쓰지 않는 힙 메모리를 런타임이 자동 회수하는 것. 회수가 도는 순간 프레임이 튀므로, 매 프레임 할당(GC Alloc)을 0으로 만드는 것이 실시간 앱의 규율.',
    '"쓰레기 수거"라는 은유 그대로 — 수거차가 도는 동안 도로(메인 스레드)가 막힌다.');

  // 시뮬레이터 통합
  T(['웨이포인트'], 'waypoint',
    '경로를 이루는 중간 지점. 웨이포인트를 노드로, 구간을 엣지로 이으면 경로 그래프가 된다.',
    'way(길)+point(지점) — 항해에서 "항로상의 지점"을 부르던 말이다.');
  T(['보간'], 'interpolation',
    '두 값(위치) 사이를 비율로 메워 중간 값을 만드는 것. 시뮬레이션의 이산 사건 사이를 부드러운 움직임으로 그리는 열쇠.',
    '라틴어 inter(사이)+polire(다듬다) — "사이를 다듬어 채운다". 한자 補間도 "사이를 기움"이다.');
  T(['상태 기계'], 'state machine',
    '개체가 가질 수 있는 상태들과 상태 사이의 전이 규칙으로 로직을 조직하는 패턴. 콜백 연쇄가 복잡해질 때의 다음 진화.',
    null);

  // 신뢰성·검증 (Phase 6)
  T(['asmdef'], 'Assembly Definition',
    'Unity에서 폴더 서브트리를 하나의 컴파일 단위(어셈블리 DLL)로 선언하는 파일. 어셈블리를 나누면 재컴파일 범위가 줄고, 의존 방향을 강제할 수 있으며, 테스트 코드를 프로덕션에서 떼어낼 수 있다.',
    'assembly는 라틴어 assimulare(함께 모으다)에서 — 여러 소스 파일이 하나의 DLL로 "모이는" 단위이고, definition은 그 경계를 파일로 "긋는다"는 뜻이다.');
  T(['회귀 테스트'], 'regression test',
    '이미 고친 결함이 다시 살아나지 않는지 자동으로 반복 확인하는 테스트.',
    'regress는 라틴어 regredi(re 뒤로 + gradi 걷다)로 "뒤로 걷다" — 코드가 옛 버그로 "되돌아가는" 것을 막는 그물이라는 뜻이다.');
  T(['EditMode', 'PlayMode'], 'EditMode / PlayMode',
    'Unity 테스트의 두 실행 모드. EditMode는 재생 없이 에디터에서 즉시 돌아 순수 로직을, PlayMode는 실제 재생 루프에서 프레임을 진행시키며 런타임 동작을 검증한다.',
    '말 그대로 "편집 중"과 "재생 중" — 프레임(시간)이 흐르느냐가 두 모드를 가르는 경계다.');
  T(['NUnit'], 'NUnit',
    '.NET 진영의 단위 테스트 프레임워크. Unity Test Framework가 이것을 바탕으로 삼아 [Test]·Assert 문법이 콘솔과 Unity에서 똑같다.',
    'N(.NET)에 자바의 원조 단위 테스트 도구 JUnit을 붙인 이름 — "닷넷판 유닛 테스트"라는 뜻이다.');
  T(['픽스처'], 'fixture (SetUp / TearDown)',
    '각 테스트 직전·직후에 공통 준비와 정리를 수행하는 장치. SetUp이 매 테스트 시작 전, TearDown이 끝난 뒤 돈다.',
    '라틴어 figere(고정하다)에서 — 매 테스트가 늘 같은 "고정된" 초기 상태에서 출발하도록 붙박아 둔다는 뜻이다.');
  T(['결정성'], 'determinism',
    '같은 입력(난수 시드)이면 언제나 같은 결과가 나오는 성질. DES에서는 SimEvent의 일련번호 tie-break가 이를 보장한다.',
    '라틴어 determinare(경계를 정하다)에서 — 결과가 미리 "정해져" 있어 흔들리지 않는다는 뜻이다.');
  T(['V&V'], 'Verification & Validation',
    'Verification은 "엔진을 설계대로 맞게 만들었나"(코드↔설계)를, Validation은 "현실을 반영하는 맞는 모델인가"(모델↔현실)를 묻는 두 검증 축이다.',
    'verify는 라틴어 verus(참), validate는 라틴어 validus(튼튼한)에서 — 한 글자 어원 차이가 "참되게 구현했나"와 "현실에 튼튼한가"라는 두 질문을 가른다.');
  T(['신뢰구간'], 'confidence interval',
    '참값이 그 안에 있으리라고 주어진 확률(보통 95%)로 말할 수 있는 값의 범위. 점추정치 하나 대신 불확실성의 폭을 함께 보고한다.',
    'confidence는 라틴어 confidere(굳게 믿다)에서 — 점 하나가 아니라 "믿는 폭"으로 결과를 말한다는 태도가 이름에 담겨 있다.');
  T(['warm-up'], 'warm-up / transient period',
    '시뮬레이션이 빈 상태에서 출발해 정상 부하에 이르기 전의 초기 과도 구간. 정상 상태 통계에서는 편향을 피하려 이 구간을 버린다.',
    '말 그대로 "데우기" — 엔진이 데워지기 전의 숫자는 대표성이 없다는 비유다.');
  T(['독립 반복'], 'replication',
    '난수 시드만 바꿔 같은 시나리오를 여러 번 돌리고, 각 실행을 하나의 표본으로 삼는 것. 반복 수가 통계의 자유도를 준다.',
    '라틴어 replicare(re 다시 + plicare 접다)에서 — 같은 실험을 "다시 접어" 겹쳐 본다는 뜻이다.');
  T(['공통 난수', 'CRN'], 'CRN, Common Random Numbers',
    '두 대안에 같은 난수 스트림을 흘려 우연에서 오는 차이를 상쇄하고 대안 자체의 차이만 남기는 분산 감소 기법.',
    'common(공통) + random — 우연을 두 대안이 "공유"하게 만들어 지운다는 발상이 이름 그대로다.');
  T(['정상 상태', '종료형'], 'steady-state / terminating',
    '정상 상태형 시뮬레이션은 끝없이 도는 시스템의 장기 평균이, 종료형은 명확한 끝(교대 종료)까지의 결과가 관심사다. 무엇을 재느냐가 통계 처리를 가른다.',
    'steady는 "흔들림 없는"(초기 과도가 사라진 뒤), terminating은 라틴어 terminus(경계·끝)에서 — 끝이 정해진 시뮬레이션이라는 뜻이다.');
  T(['MAPE'], 'Mean Absolute Percentage Error',
    '예측값과 실측값의 절대 백분율 오차를 평균한 지표. 트윈 KPI가 실측 로그와 몇 % 어긋나는지를 한 숫자로 말한다.',
    '이름 그대로 "평균 절대 백분율 오차" — Validation의 대표 척도다.');
  T(['스켈레톤'], 'skeleton (code)',
    '뼈대만 갖춘 코드. 그대로 컴파일되되 핵심 메서드가 TODO 스텁이라, 학습자가 살을 채우는 실습용 골격을 말한다.',
    '그리스어 skeletos(말린 것, 해골)에서 — 살이 빠지고 뼈대만 남은 형태라는 비유다.');
  T(['완료 기준', 'DoD'], 'Definition of Done',
    '어떤 작업이 "끝났다"고 말할 수 있는 조건을 미리 못 박아 둔 목록. 주관이 아니라 합의된 정의로 "끝"을 고정한다.',
    'done은 "행해진(끝난)" — "끝"의 기준을 정의로 붙박아 둔다는 뜻이다.');

  // Fleet 심화 — 교통·데드락·라우팅·배차 (Phase 7)
  T(['구간 점유'], 'zone control',
    '경로망을 구간(존)으로 나눠 한 구간에는 한 대만 들이고, 차량은 다음 구간을 미리 예약받아야 진입하는 교통 제어 방식이다. 철도의 폐색 제어와 같은 원리다.',
    'zone은 그리스어 zōnē(허리띠·둘러친 구역)에서 왔다 — 길을 띠처럼 토막 내어 하나씩 잠근다는 발상이 이름에 그대로 들어 있다.');
  T(['폐색'], 'blocking',
    '앞 구간이 이미 점유되어 차량이 구간 경계에서 더 나아가지 못하고 멈춰 기다리는 상태다.',
    'block은 중세 네덜란드어 blok(길을 가로막는 통나무 덩어리)에서 왔고, 철도의 "block system(폐색)"이 여기서 나왔다.');
  T(['밀도-흐름 관계'], 'fundamental diagram',
    '교통 밀도(단위 길이당 차량 수)와 흐름(단위 시간당 통과 수)의 관계 곡선으로, 임계 밀도까지는 흐름이 늘다가 그 뒤 정체로 급감한다.',
    'fundamental은 라틴어 fundus(바닥)에서 왔다 — 교통공학이 다른 모든 분석의 바닥에 두는 "기본" 그림이라는 뜻이다.');
  T(['기아'], 'starvation',
    '자원을 계속 다른 요청에 양보당해 특정 주체가 무한정 처리되지 못하는 상태다.',
    'starve는 고대 영어 steorfan(죽다)에서 왔고 뜻이 "굶어 죽다"로 좁혀졌다 — 자원을 얻지 못해 굶는 프로세스에 빗댄 말이다.');
  T(['우선순위 역전'], 'priority inversion',
    '낮은 우선순위 작업이 자원을 쥐고 있어, 높은 우선순위 작업이 오히려 그를 기다리게 되는 뒤집힘이다.',
    'invert는 라틴어 in-(안으로)+vertere(돌리다)로 "뒤집다" — 우선순위의 순서가 거꾸로 서는 현상을 가리킨다.');
  T(['점유 대기'], 'hold-and-wait',
    '자원을 하나 쥔 채 다른 자원을 기다리는 상태로, 데드락 성립 4조건 중 하나다.',
    '말 그대로 hold(쥐다)+wait(기다리다) — 쥔 것을 놓지 않은 채 더 달라고 기다린다는 뜻이다.');
  T(['순환 대기'], 'circular wait',
    '대기 관계가 A→B→…→A로 원을 이루는 상태로, 데드락 성립 4조건 중 마지막이다.',
    'circular는 라틴어 circulus(작은 원)에서 왔다 — 기다림이 원을 그리면 빠져나갈 출구가 없다.');
  T(['wait-for 그래프'], 'wait-for graph',
    '각 주체를 노드로, "누가 누구를 기다리는가"를 간선으로 그린 방향 그래프로, 사이클이 존재하면 곧 데드락이다.',
    '영어 "wait for(~를 기다리다)"를 그대로 그래프 이름으로 삼은 것으로, 간선의 방향이 곧 기다림의 방향이다.');
  T(['뱅커스 알고리즘'], "Banker's algorithm",
    '자원 요청을 승인해도 시스템이 여전히 "안전 상태"(모두가 언젠가 끝날 수 있는 상태)로 남는지 미리 검사해, 아니면 보류하는 데드락 회피 기법이다.',
    '다익스트라가 은행가에 빗대 이름 붙였다 — 은행이 모든 예금주의 인출을 언젠가 다 갚을 수 있을 때만 대출을 내주는 것과 같다.');
  T(['자원 순서화'], 'resource ordering',
    '모든 자원에 전역 번호를 매기고 항상 번호가 커지는 순서로만 획득하게 해, 순환 대기를 원천 차단하는 데드락 예방 기법이다.',
    'order는 라틴어 ordo(줄·차례)에서 왔다 — 자원에 줄을 세워 두면 대기 관계가 원을 그릴 수 없다.');
  T(['A*'], 'A-star',
    '지금까지의 실비용 g와 목표까지의 추정 h를 더한 f=g+h로 확장 순서를 정해, 탐색을 목표 쪽으로 당기는 최단경로 알고리즘이다.',
    '1968년 고안자들이 최적성이 증명된 버전에 별표(*)를 붙여 A*라 불렀다 — 별표는 "증명된 최적"을 뜻하는 수학 표기다.');
  T(['허용 가능 휴리스틱'], 'admissible heuristic',
    '어느 노드에서도 목표까지의 실제 최소 비용을 절대 넘겨 추정하지 않는(과소평가만 하는) 휴리스틱으로, 이 성질이 있어야 A*가 최단해를 보장한다.',
    'admit은 라틴어 ad-(~쪽으로)+mittere(보내다)로 "들여보내다" → "받아들일 만한". heuristic은 그리스어 heuriskein(찾아내다)에서 온, "유레카"와 같은 뿌리다.');
  T(['재계획'], 'rerouting',
    '주행 중 남은 경로를 다시 계산해 더 나은 길로 갈아타는 것으로, 정체나 예약 실패가 누적될 때 촉발된다.',
    're-(다시)+route이며, route는 라틴어 rupta via(뚫린 길)에서 왔다 — 막힌 상황에서 길을 다시 뚫는다는 뜻이다.');
  T(['시간창 예약'], 'time-window reservation',
    '엣지나 노드를 "언제부터 언제까지" 시간 구간 단위로 미리 예약해, 충돌을 사후 회피가 아니라 사전에 배제하는 방식이다.',
    'reserve는 라틴어 re-(뒤로)+servare(지켜 두다)로 "따로 떼어 지켜 두다"이며, window는 시간 축에 열린 구간을 창에 빗댄 것이다.');
  T(['WHCA*'], 'Windowed Hierarchical Cooperative A*',
    '여러 에이전트가 시간축을 포함한 예약표를 공유하며, 정해진 시간창 안에서만 협력적으로 A* 경로를 잡는 다중 에이전트 경로 탐색 기법이다.',
    '이름이 곧 구성요소의 나열이다 — windowed(시간창)+hierarchical(계층)+cooperative(협력)+A*.');
  T(['CBS'], 'Conflict-Based Search',
    '각 에이전트의 최단 경로를 먼저 구한 뒤, 충돌이 나는 쌍에만 제약을 추가해 다시 푸는 2계층 다중 에이전트 경로 탐색이다.',
    'conflict는 라틴어 confligere(함께 부딪치다)에서 왔다 — 부딪힘(충돌)을 기준으로 탐색을 분기한다.');
  T(['에이징'], 'aging',
    '오래 기다린 요청의 우선순위를 대기 시간에 비례해 점점 올려, 탐욕적 선택이 특정 요청을 영원히 미루는 기아를 막는 기법이다.',
    'age(나이)+-ing으로, 요청이 "나이를 먹을수록" 대접이 좋아진다는 뜻이 그대로 담겨 있다.');
  T(['핫 로트'], 'hot lot',
    '납기나 중요도가 급해 일반 로트를 제치고 우선 처리되는 생산 로트다.',
    'hot은 "뜨거운"에서 "긴급한·주목받는"으로 뜻이 확장됐고(hot topic처럼), lot은 중세 영어에서 "한 묶음의 물량"이다.');
  T(['P95'], '95th percentile',
    '표본을 크기순으로 놓았을 때 95 %가 그 값 이하인 지점으로, 평균이 감추는 꼬리(최악 근처)의 경험을 드러내는 지표다.',
    'percentile은 라틴어 per centum(백당)에서 왔다 — 100등분한 눈금의 95번째 칸이다.');
  T(['임계치 충전'], 'threshold charging',
    '배터리가 정해진 임계값 아래로 떨어지면 차량을 충전소로 보내는 단순한 충전 정책이다.',
    'threshold는 고대 영어 therscold(문지방)에서 왔다 — 넘어서면 안 되는 문턱을 가리킨다.');
  T(['기회 충전'], 'opportunity charging',
    '유휴 시간이 생길 때마다 근처 충전소에서 조금씩 충전해 배터리를 늘 넉넉하게 유지하는 정책이다.',
    'opportunity는 라틴어 ob portum(항구를 향해)에서 왔다 — 순풍이 배를 항구로 밀어주는 좋은 때, 곧 "기회"다.');

  // 디지털 트윈 · 네트워킹 · UI (Phase 8)
  T(['미러링'], 'mirroring',
    '실물의 상태를 화면에 단방향으로 비춰 보여 주기만 하는 디지털 트윈의 가장 낮은 성숙도 단계. 관찰만 하고 실물로의 되먹임은 없다.',
    '거울을 뜻하는 mirror에서 왔다. 거울이 앞의 사물을 그대로 되비추듯 실물을 화면에 비추기만 한다는 뜻으로, mirror의 뿌리는 라틴어 mirari(경탄하며 바라보다)다.');
  T(['섀도', '디지털 섀도'], 'digital shadow',
    '실물의 이력을 축적·분석까지 하지만 실물을 제어하지는 않는 트윈의 중간 성숙도 단계. 미러링과 완전한 트윈 사이에 있다.',
    '그림자를 뜻하는 shadow에서 왔다. 그림자는 사물을 따라다니되 사물을 바꾸지는 못한다 — 실물을 좇아 기록하지만 되먹이지 않는 이 단계의 성격이 이름에 담겨 있다.');
  T(['NDJSON'], 'Newline-Delimited JSON',
    '한 줄에 완결된 JSON 객체 하나를 담고 줄바꿈으로 구분하는 스트리밍 로그 형식. 한 줄씩 흘려보내고 받아 파싱할 수 있어 이벤트 스트림에 알맞다.',
    'newline(줄바꿈)·delimited(구분된)·JSON의 합성이다. "줄바꿈으로 구분된 JSON"이라는 형식의 규칙이 이름 그대로 박혀 있다.');
  T(['텔레메트리'], 'telemetry',
    '멀리 떨어진 장비의 상태를 측정해 원격지로 전송하는 것, 또는 그렇게 전송된 데이터. 트윈이 받아 그리는 실물 피드가 곧 텔레메트리다.',
    '그리스어 tele(멀리)와 metron(측정)의 합성으로 "멀리서 재기"라는 뜻이다. 사람이 곁에 갈 수 없는 우주선·발전소의 계기값을 읽어 보내던 데서 굳어졌다.');
  T(['이벤트 소싱'], 'event sourcing',
    '현재 상태를 직접 저장하는 대신, 상태를 바꾼 사건들의 연속을 원본으로 기록해 두고 그것을 재생해 어느 시점의 상태든 되살리는 방식.',
    'event(사건)를 진리의 source(원천)로 삼는다는 뜻이다. 상태가 아니라 "무슨 일이 있었는가"의 목록이 원본이라는 발상이 이름에 담겼다.');
  T(['스냅샷'], 'snapshot',
    '어느 한 시각의 전체 상태를 통째로 담은 자기 완결적 메시지. 이전 맥락 없이도 그 자체로 상태를 복원할 수 있어 재동기화와 뒤늦은 합류의 기준점이 된다.',
    'snap(찰칵)과 shot(찍기)의 합성이다. 원래 사냥에서 겨냥 없이 재빨리 쏘는 "속사"를 뜻하다가, 순간을 통째로 박제하는 사진을 가리키게 됐다.');
  T(['델타'], 'delta',
    '직전 상태로부터 무엇이 바뀌었는지만 담은 변화분. 전체 대신 델타만 흘려보내면 대역폭을 아끼지만, 기준이 되는 이전 상태가 있어야 뜻이 통한다.',
    '그리스 문자 Δ(델타)가 수학에서 두 값의 "차이"를 나타내는 관행에서 왔다. 삼각형 모양의 이 문자는 나일강 어귀의 삼각주(delta)에도 같은 이름을 주었다.');
  T(['플레이아웃 지연'], 'playout delay',
    '도착한 데이터를 즉시 그리지 않고 버퍼에 잠시 담아 두었다가 일정 시간 뒤에 재생하는 것. 이 짧은 지연이 네트워크 흔들림을 흡수해 화면을 매끄럽게 만든다.',
    'play out(끝까지 내보내 재생하다)과 delay(지연)의 합성이다. 매끄러움을 얻기 위해 일부러 지불하는 시간이라는 뜻이 담겼다.');
  T(['지터 버퍼'], 'jitter buffer',
    '도착 간격이 들쭉날쭉한(지터) 데이터를 잠시 모아 두었다가 고른 간격으로 내보내는 완충 저장소. 플레이아웃 지연을 실현하는 장치다.',
    'jitter는 "초조하게 떨다"라는 뜻의 의성적 영어 단어로 불규칙한 떨림을 가리키고, buffer는 충격을 흡수하는 완충기다 — 떨림을 눌러 고르게 펴는 그릇이라는 이름이다.');
  T(['WebSocket'], 'WebSocket',
    '한 번 연결을 맺으면 서버와 브라우저가 양방향으로 계속 메시지를 주고받는 통신 규약. 브라우저 대시보드에 실시간 피드를 밀어 넣을 때 쓴다.',
    'web(월드 와이드 웹)과 socket(연결 구멍)의 합성이다. socket은 무언가를 꽂아 잇는 구멍을 뜻하며, 여기서는 웹 위에 상시 열려 있는 연결 통로를 가리킨다.');
  T(['마샬링'], 'marshalling',
    '한 실행 맥락(예: 백그라운드 수신 스레드)에서 만든 데이터나 호출을 다른 맥락(예: Unity 메인 스레드)으로 안전하게 넘기는 것. 스레드 경계를 넘길 때 반드시 거친다.',
    '뒤섞인 것을 줄 세워 이끄는 사람인 marshal에서 온 동사다. "말을 돌보는 사람"이 궁정의 서열을 정리하는 직책으로 올라간 데서 왔다.');
  T(['UXML'], 'UnityXML',
    'Unity UI Toolkit에서 화면의 구조와 요소 계층을 정의하는 XML 마크업. 웹의 HTML에 해당한다.',
    'Unity와 XML(eXtensible Markup Language)의 합성이다. 구조를 태그로 적는다는 점에서 마크업 언어의 계보에 놓인다.');
  T(['USS'], 'Unity Style Sheets',
    'UXML로 짠 UI 요소에 색·여백·배치 같은 스타일을 입히는 시트. 웹의 CSS에 해당한다.',
    'Unity Style Sheets의 머리글자다. 이름도 역할도 웹의 CSS(Cascading Style Sheets)를 본떠 만들어졌다.');
  T(['BEM'], 'Block Element Modifier',
    'UI 클래스 이름을 블록·요소·수정자 세 조각으로 나눠 block__element--modifier처럼 짓는 명명 규약. 이름만 보고 구조와 상태를 읽을 수 있다.',
    'Block(독립 컴포넌트)·Element(그 안의 요소)·Modifier(변형·상태)의 머리글자다. 이름 짓는 규칙 자체가 이 세 낱말의 순서로 굳어 있다.');
  T(['ListView 가상화'], 'ListView virtualization',
    '목록이 아무리 길어도 화면에 실제로 보이는 몇 행만 만들어 재활용하는 기법. 차량 수천 대 목록도 눈에 보이는 십여 개 행만 그려 가볍게 유지한다.',
    '실재하지 않지만 있는 듯 작동함을 뜻하는 virtual에서 온 virtualization이다. 수천 개가 다 있는 것처럼 보이지만 실제로는 보이는 것만 존재한다는 착시가 이름의 핵심이다.');

  // 대규모 성능 — Jobs·메모리·GPU (Phase 9)
  T(['NativeArray'], 'NativeArray<T>',
    'GC가 관리하지 않는 네이티브 메모리에 값 타입을 연속 배치한 배열. 잡에 넘겨 워커 스레드에서 안전하게 읽고 쓸 수 있다.',
    'native는 라틴어 nativus(타고난)에서 왔다 — C# 관리 힙 바깥, 엔진이 직접 잡는 "본토" 메모리라 GC의 손이 닿지 않는다는 뜻이다.');
  T(['Allocator'], 'Allocator',
    'NativeArray가 살아 있는 기간을 정하는 메모리 할당 정책이다(Temp/TempJob/Persistent).',
    'allocate는 라틴어 ad(향해)+locare(두다), 곧 "자리를 배정하다"이다 — 그 자리를 얼마나 오래 지킬지가 세 종류의 차이다.');
  T(['Burst'], 'Burst',
    'C#/IL로 짠 잡 코드를 LLVM으로 받아 SIMD까지 활용하는 네이티브 기계어로 컴파일하는 Unity 컴파일러다.',
    'burst는 "한꺼번에 터져 나오다"이다 — 명령 하나로 여러 데이터를 동시에 처리하는 SIMD의 폭발적 처리량을 이름에 담았다.');
  T(['SIMD'], 'Single Instruction, Multiple Data',
    '명령 하나로 여러 데이터를 동시에 연산하는 CPU 병렬화 방식이다.',
    'single(하나의) instruction에 multiple(여럿) data — 이름이 곧 정의다. Flynn의 병렬 컴퓨터 분류(1966)에서 온 약어다.');
  T(['워커 스레드'], 'worker thread',
    '메인 스레드 대신 잡을 나눠 실행하는 백그라운드 스레드다. Unity는 CPU 코어 수에 맞춰 워커 풀을 둔다.',
    'thread는 "실"로 하나의 실행 흐름을 실오라기에 빗댄 말이고, worker는 그 실을 대신 짜 주는 일꾼이다.');
  T(['TransformAccessArray'], 'TransformAccessArray',
    'GameObject의 Transform 무리를 잡에서 병렬로 읽고 쓰게 해 주는 특수 컨테이너로, IJobParallelForTransform과 짝을 이룬다.',
    'Transform에 대한 access(접근)를 배열로 묶은 것이다 — GameObject를 버리지 않고도 대량 이동을 잡으로 넘기는 다리다.');
  T(['JobHandle'], 'JobHandle',
    '예약한 잡의 완료를 추적하고 잡 사이 의존 관계를 잇는 손잡이다. Complete()로 결과를 기다린다.',
    'handle은 "손잡이"다 — 실행 중인 잡을 붙잡아 두고, 이 손잡이로 "이 잡은 저 잡 다음"이라는 순서를 건다.');
  T(['ProfilerMarker'], 'ProfilerMarker',
    '코드의 특정 구간을 Profiler 타임라인에 이름표로 찍어 그 구간만의 소요 시간을 재는 계측 도구다.',
    'marker는 "표시자"다 — 재고 싶은 구간의 시작과 끝에 꽂는 깃발이고, Profiler가 그 깃발 사이 시간을 잰다.');
  T(['Memory Profiler'], 'Memory Profiler',
    '실행 중 메모리 스냅샷을 찍어 관리·네이티브·자산별 점유를 뜯어보고, 스냅샷을 비교해 누수를 사냥하는 패키지다.',
    'profile은 이탈리아어 profilo(윤곽)에서 왔다 — 메모리의 윤곽을 그려 어디가 부풀었는지 본다는 뜻이다.');
  T(['애디티브 씬'], 'additive scene',
    '기존 씬을 유지한 채 위에 겹쳐 로드하는 씬이다. 큰 맵을 구역별로 나눠 필요한 곳만 메모리에 올린다.',
    'add(더하다)에서 왔다 — 씬을 "교체"하지 않고 "더한다"는 동작이 이름 그대로다.');
  T(['Addressables'], 'Addressables',
    '자산에 주소(문자열 키)를 붙여 비동기로 로드·해제하고 참조 카운트로 수명을 관리하는 Unity 패키지다.',
    'address(주소)+-able, 곧 "주소로 부를 수 있는" 자산이다 — 빌드에 통째로 박는 대신 이름표로 불러온다.');
  T(['GraphicsBuffer'], 'GraphicsBuffer / ComputeBuffer',
    'CPU와 GPU가 함께 읽고 쓰는 구조화된 GPU 메모리로, 컴퓨트 셰이더의 입출력이자 인스턴스별 데이터의 그릇이다.',
    'buffer는 "완충 장치"다 — CPU와 GPU 사이에 데이터를 쟁여 두는 공용 창고라는 뜻이다.');
  T(['인다이렉트 드로우'], 'indirect draw',
    '그릴 인스턴스 수·오프셋을 CPU가 아닌 GPU 버퍼에서 읽어 드로우콜 한 번으로 수만 개를 그리는 렌더링이다.',
    'indirect는 "간접적"이다 — 그리기 인자를 CPU가 직접 넘기지 않고 버퍼를 거쳐 전달하므로, CPU는 손을 떼고 GPU가 스스로 반복한다.');
  T(['DOTS'], 'Data-Oriented Technology Stack',
    '데이터를 캐시 친화적으로 배치해 대량 엔티티를 처리하는 Unity의 데이터 지향 스택(ECS)으로, Jobs·Burst 위에 선다.',
    'entity(개체)를 component(데이터 조각)의 조합으로 보고 system(로직)이 훑는 구조이며, DOTS는 data-oriented(데이터 지향) 철학의 머리글자다.');
  T(['VSync'], 'Vertical Synchronization',
    '프레임 표시를 모니터 주사율에 맞춰 동기화하는 설정으로, 켜면 프레임레이트가 주사율에 캡되어 CPU 여유를 가린다(측정 시 끈다).',
    'vertical(수직) sync는 화면을 위에서 아래로 다 그린 뒤 다음 프레임으로 넘어가는 수직 귀선 시점에 맞춘다는 뜻으로, CRT 시절 주사선이 수직으로 되돌아오던 순간에서 온 말이다.');

  // 실자산 파이프라인 (Phase 10)
  T(['tessellation', '테셀레이션'], 'tessellation',
    'CAD의 수학적 곡면(B-rep)을 렌더링할 수 있는 삼각형 그물로 잘게 나누는 과정. 곡률이 클수록 더 촘촘히 쪼개 곡면을 근사한다.',
    '라틴어 tessella(작은 모자이크 돌조각)에서 왔다. 로마 바닥 모자이크를 이루던 작은 정사각 돌처럼, 곡면을 작은 조각으로 덮는다는 뜻이 이름에 남아 있다.');
  T(['서브메시'], 'submesh',
    '하나의 메시 안에서 같은 재질이 적용되는 삼각형 묶음. 메시가 재질을 N개 쓰면 서브메시 N개로 갈라지고, 서브메시 하나가 최소 드로우콜 하나가 된다.',
    'sub(하위)+mesh(그물). mesh는 그물코를 뜻하던 옛말로, 삼각형이 얽힌 표면이 그물을 닮아 붙었다.');
  T(['Read/Write Enabled'], 'Read/Write Enabled',
    '메시 데이터를 런타임에 CPU에서도 읽고 쓸 수 있게 유지하는 임포트 옵션. 켜면 같은 메시가 CPU·GPU 양쪽에 상주해 메모리를 두 배로 쓴다.',
    '말 그대로 "읽기/쓰기 허용". 스크립트가 정점을 만지거나 콜라이더·NavMesh를 구울 때만 켜라는 경고가 이름에 담겨 있다.');
  T(['mipmap', '밉맵'], 'mipmap',
    '텍스처를 절반씩 줄인 사본을 미리 만들어 두고, 화면에서 멀어질수록 작은 사본을 골라 쓰는 기법. 원거리의 반짝임(앨리어싱)과 메모리 대역폭을 줄인다.',
    '라틴어 multum in parvo("작은 것 안에 많이")의 머리글자 MIP에 map(지도)을 붙인 말. 1983년 랜스 윌리엄스가 이 이름을 지었다.');
  T(['sRGB'], 'standard RGB',
    '사람 눈의 밝기 감각에 맞춰 색을 비선형으로 저장하는 표준 색 공간. 베이스컬러 텍스처는 sRGB로, 노멀·마스크 같은 데이터 텍스처는 Linear로 다뤄야 색이 맞는다.',
    'standard RGB의 약자. 1996년 HP와 마이크로소프트가 모니터 색을 표준화하려고 제정한 규격의 이름이 그대로 굳었다.');
  T(['BC7'], 'Block Compression 7',
    'GPU가 압축을 푼 채로 그대로 읽는 블록 단위 텍스처 압축 포맷의 하나. 고품질 컬러에 쓰고, 알파 없는 저용량엔 BC1(옛 DXT1)을 쓴다.',
    'Block Compression의 7번 변형 — 텍스처를 4×4 픽셀 블록으로 나눠 압축한다는 뜻. DXT는 이 방식의 옛 DirectX 시절 이름이다.');
  T(['Weld Vertices', '정점 용접'], 'weld vertices',
    '위치가 같은 정점들을 하나로 합치는 임포트 옵션. 불필요하게 쪼개진 정점을 줄여 메시를 가볍게 한다.',
    'weld는 "용접하다". 쇠를 녹여 잇듯 같은 자리에 겹친 정점을 하나로 붙인다는 비유다.');
  T(['프리팹 Variant'], 'prefab variant',
    '기존 프리팹을 원본으로 삼아 오버라이드만 얹어 파생한 프리팹. 원본(임포트된 모델)이 갱신돼도 내가 더한 콜라이더·재질 변경이 유지된다.',
    'variant는 라틴어 variare(바꾸다)에서 온 "변형판". 원본을 복제하지 않고 "다르게 만든 판"이라는 뜻이다.');
  T(['B-rep'], 'boundary representation',
    '물체를 채워진 부피가 아니라 그것을 감싸는 면·모서리·꼭짓점의 수학적 경계로 기술하는 CAD 표현. 렌더링하려면 tessellation으로 삼각형화해야 한다.',
    'Boundary Representation의 준말 — "경계로 나타내기". 속을 채우는 대신 껍데기의 수식으로 형상을 정의한다.');
  T(['AssetPostprocessor'], 'AssetPostprocessor',
    '자산을 임포트하는 순간 Unity가 자동으로 호출하는 에디터 훅 클래스. 상속만 하면 등록 없이 임포트 파이프라인에 끼어들어 스케일·재질·플래그를 교정한다.',
    'asset(자산)+post(뒤에)+processor(처리기) — "임포트 뒤에 자산을 손보는 것". post는 라틴어로 "~후에".');
  T(['EditorWindow'], 'EditorWindow',
    '에디터 안에 띄우는 커스텀 창의 기반 클래스. OnGUI로 원하는 UI를 그려 도구나 리포트 창을 만든다.',
    'editor(편집기)에 붙는 window(창) — 에디터를 확장하는 도구 창이라는 뜻 그대로다.');
  T(['ModelImporter'], 'ModelImporter',
    'FBX·OBJ 같은 모델 파일의 임포트 설정을 담는 객체. 스케일·노멀·재질 위치 등을 코드로 바꿀 때 이 객체를 통한다.',
    'model(모형)+importer(들여오는 것) — 모델을 프로젝트로 들여오는 설정 창구.');
  T(['MeshCollider'], 'mesh collider',
    '메시의 실제 형상을 그대로 충돌 판정에 쓰는 콜라이더. 정밀하지만 비싸서 움직이지 않는 정적 지오메트리에 주로 붙인다.',
    'mesh(메시)+collider(충돌체). collide는 라틴어 collidere(맞부딪치다) — "메시 모양의 부딪침 판정기".');
  T(['스태틱 플래그'], 'static flags',
    '오브젝트가 런타임에 움직이지 않음을 Unity에 알리는 표식. 정적 배칭·라이트맵·오클루전 컬링 같은 사전 계산 최적화의 대상이 된다.',
    'static은 그리스어 statikos(멈춰 선)에서 온 "고정된". flag는 깃발처럼 세워 두는 표식이라는 뜻이다.');

  // ---------- 본문 주석(annotation) ----------
  var EXCLUDE = 'a, code, pre, h1, h2, h3, svg, script, style, summary, .en, .term, .eyebrow, .rail-legend, .brand, .crumb';

  function esc(s) { return s.replace(/[.*+?^${}()|[\]\\/]/g, '\\$&'); }
  var keys = Object.keys(D).sort(function (a, b) { return b.length - a.length; });
  var RE = new RegExp('(?<![A-Za-z0-9가-힣])(' + keys.map(esc).join('|') + ')(?![A-Za-z0-9])', 'g');

  function annotateElement(el, seen) {
    var walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT, {
      acceptNode: function (node) {
        if (!node.nodeValue || !node.nodeValue.trim()) return NodeFilter.FILTER_REJECT;
        if (node.parentElement && node.parentElement.closest(EXCLUDE)) return NodeFilter.FILTER_REJECT;
        return NodeFilter.FILTER_ACCEPT;
      }
    });
    var nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);

    nodes.forEach(function (node) {
      var text = node.nodeValue;
      RE.lastIndex = 0;
      var m, parts = [], last = 0;
      while ((m = RE.exec(text)) !== null) {
        var entry = D[m[1]];
        if (!entry || seen.has(entry)) continue;
        seen.add(entry);
        parts.push(document.createTextNode(text.slice(last, m.index)));
        var span = document.createElement('span');
        span.className = 'term';
        span.textContent = m[1];
        span.setAttribute('data-term', m[1]);
        span.setAttribute('tabindex', '0');
        parts.push(span);
        last = m.index + m[1].length;
      }
      if (!parts.length) return;
      parts.push(document.createTextNode(text.slice(last)));
      var frag = document.createDocumentFragment();
      parts.forEach(function (p) { frag.appendChild(p); });
      node.parentNode.replaceChild(frag, node);
    });
  }

  // ---------- 툴팁 UI ----------
  var tip = null;
  function ensureTip() {
    if (tip) return tip;
    tip = document.createElement('div');
    tip.id = 'term-tip';
    tip.setAttribute('role', 'tooltip');
    document.body.appendChild(tip);
    return tip;
  }

  function showTip(target) {
    var entry = D[target.getAttribute('data-term')];
    if (!entry) return;
    var t = ensureTip();
    t.innerHTML =
      '<div class="t-name">' + entry.name + ' <span class="t-en">' + entry.en + '</span></div>' +
      '<p class="t-def">' + entry.def + '</p>' +
      (entry.ety ? '<p class="t-ety"><span class="t-label">어원</span> ' + entry.ety + '</p>' : '');
    t.style.display = 'block';
    var r = target.getBoundingClientRect();
    var tw = Math.min(360, window.innerWidth - 24);
    t.style.maxWidth = tw + 'px';
    var tr = t.getBoundingClientRect();
    var left = Math.min(Math.max(8, r.left), window.innerWidth - tr.width - 8);
    var top = r.top - tr.height - 10;
    if (top < 8) top = r.bottom + 10;
    t.style.left = left + 'px';
    t.style.top = top + 'px';
  }

  function hideTip() { if (tip) tip.style.display = 'none'; }

  function init() {
    var main = document.querySelector('main');
    if (!main) return;
    var seen = new Set();
    Array.prototype.forEach.call(main.children, function (child) {
      if (child.tagName === 'H2') { seen = new Set(); return; }
      annotateElement(child, seen);
    });

    document.addEventListener('mouseover', function (e) {
      var t = e.target.closest && e.target.closest('.term');
      if (t) showTip(t);
    });
    document.addEventListener('mouseout', function (e) {
      if (e.target.closest && e.target.closest('.term')) hideTip();
    });
    document.addEventListener('focusin', function (e) {
      var t = e.target.closest && e.target.closest('.term');
      if (t) showTip(t);
    });
    document.addEventListener('focusout', function (e) {
      if (e.target.closest && e.target.closest('.term')) hideTip();
    });
    // 모바일: 탭으로 토글, 바깥 탭으로 닫기
    document.addEventListener('click', function (e) {
      var t = e.target.closest && e.target.closest('.term');
      if (t) {
        if (tip && tip.style.display === 'block') hideTip(); else showTip(t);
      } else if (!(e.target.closest && e.target.closest('#term-tip'))) {
        hideTip();
      }
    });
    window.addEventListener('scroll', hideTip, { passive: true });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
