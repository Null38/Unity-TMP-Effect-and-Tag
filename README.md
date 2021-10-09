# Uniyt TMP를 이용한 효과및 태그 시스템
2020.1.3f1 버전에서 만들었기에 이전버전에서는 안될 가능성도 존재합니다.
TextController.cs, TextController.cs가 사실상 전부입니다.

다운로드 : <https://github.com/Null38/Unity-TMP-Effect-and-Tag/releases/tag/1>

## 태그 종류
그냥 꺽쇠괄호를 사용하려면 "\\<"를 사용해야 합니다. 안그러면 에러가 납니다.
* <\~> 출렁이기 </\~> 
* <\*> 흔들기 </\*>
* <%> 회전시키기 </%>
* <!> 임팩트 </!> (타이핑 이펙트)
* <'' (초)> 타이핑 속도 <''>
* \<big> 큰 글씨 \</big> 25% 큰 글씨

## 사용법
글씨 색은 4개로 되어있는데 꼭지점 별로 색 변경을 위해서 입니다.

타이핑 효과는 위치랑 크기, 글자색을 변경할 수 있습니다.

모든 속도의 단위는 (초)입니다.

### ● TextController.cs
샘플씬에는 캔버스에 들어가 있습니다.

base color : 기본 글씨색 입니다.

typingTransform : 기본 타이핑 형태 입니다.

impactTypingTransform : 임팩트 타이핑 형태 입니다.

typingSpeed : 타이핑 속도입니다. 

typingDelay : 다음 글자가 나타나는 속도 입니다. 

typingRestDelay : 타이핑 휴식 길이 입니다. 미계발 입니다.(".",","같은 것에 들어갈 예정.)


shakePower : 글자를 흔드는 세기 입니다. 클수록 강합니다.

shakeDelay : 글자를 흔드는 속도 입니다. 

wavePower : 글자가 출렁이는 진폭의 크기입니다.

waveGap : 글자가 출렁이는 파장의 크기입니다.

waveSpeed : 글자가 출렁이는 속도입니다.

rotateAngle : 기울어진 글자의 최대 각도의 절대값입니다. 60분법을 이용합니다.



글자 기울이기에는 함수가 사용되어있습니다. 밑의 두개의 변수가 시드라 보시면 됩니다. 
* rotateRandomA
* rotateRandomB

script tag : 스크립트로 단순 태그를 추가할때 사용됩니다. 심플씬에는 ~, !, \*, %가 할당되어 있습니다.

change tag : 태그를 다른 글로 변환합니다. 심플씬에는 big태그와 <''>태그를 만들때 사용되었습니다

### ● TextController.cs
샘플씬에는 Text(TMP)에 들어가 있습니다.


textComponent : TMP컴포넌트를 집어넣어야 합니다.

effectReaders : 글의 태그와 속도가 들어가 있습니다. 무시하셔도 됩니다

txMana : TextManager컴포넌트를 집어넣어야 합니다. TextManager에 들어있는 수치값을 이용하기 위하여 사용됩니다.

만약 타이핑을 스킵하고 싶으시다면 코드에서 TextTyping()의 typingTextCounter값을 textInfo.characterCount의 값으로 변경하시면 됩니다.
