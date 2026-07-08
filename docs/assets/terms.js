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
