"use strict"

function TestArr1(){
    const arr1 = new Array(1, 4, 5, 9);
    const arr2 = Array(1, 4, 5, 9);
    const arr3 = [1,4,5,9];

    console.log(arr1)
}

function TestArr2(){
    const arr1 = new Array(5);
    const arr2 = Array(5);
    const arr3 = [];
    arr3.length = 5;
}

function TestArr3(){
    const obj = {};
    obj.song = [];
    // length는 안 써줘도 알아서 되긴 함
    obj.song.length = 3;
    obj.song[0] = 2;
    obj.song[1] = 5;
    obj.song[2] = 10;

    const obj2 = {
        song: [2, 5, 10],
        kim: "재원",
        lim: 5,
    };

    for(let i = 0; i < obj.song.length; i++){
        console.log(obj.song[i]);
    }

    // 생긴건 foreach랑 비슷함 기능은 그냥 for문임 [in] 씀
    for(let ele in obj.song){
        console.log(obj.song[ele]);
    }

    // 그냥 foreach임 [of] 씀
    for(let ele of obj.song){
        console.log(ele);
    }

    // foreach는 배열 안에 있는 함수임, 괄호 안에는 콜백 함수
    obj.song.forEach((k) => {
        console.log(k);
    });
}

function TestArr4(){
    // indexOf 입력받은 데이터가 첫번쨰로 나타나는 위치의 인덱스 반환
    // 파라미터에 데이터 넣어줌 (대소문자 구분함)
    var arr = ['a', 'b', 'c', 'd'];
    console.log(arr.indexOf('c'));  // 2

    // toString number타입도 문자열화 시켜줌
    var arr2 = [1, '2', [1,2,3], {name : 'lee'}];
    console.log(arr2.toString()); // 1,2,1,2,3,[object object]

    // push 배열의 마지막에 원소 추가
    // 반환값은 배열의 길이 반환 (새로넣은거 포함)
    var arr3 = new Array('a', 'b', 'c', 'd');
    arr3.push('e');
    console.log(arr3); // ['a', 'b', 'c', 'd', 'e']
    console.log(arr3.push('f')); // 6

    // shift 배열의 첫번째 원소를 제거
    // 제거한 데이터는 해장 원소 타입으로 반환
    var arr4 = ['a', 'b', 'c', 'd'];
    arr4.shift(); // a 없어짐
    console.log(arr4.shift()); // b 출력
    console.log(arr4); // ['c', 'd'];

    // unshift 배열의 첫번째에 원소 추가
    // 반환값은 늘어난 배열의 길이
    var arr5 = new Array('a', 'b', 'c', 'd');
    arr5.unshift('b1'); // 배열 맨앞에 b1 추가
    console.log(arr5.unshift('a0')); // 6
    console.log(arr5); // ['a0', 'b1', 'a', 'b', 'c', 'd']

    // sort 기본적으로 배열의 원소를 오름차순으로 정렬
    var arr6 = ['f', 4, '라', 2, 'c', '나', [1,5,3]];
    console.log(arr6.sort()); // [ [1,5,3], 2, 4, 'c', 'f', '나', '라']

    // reverse 배열의 순서를 뒤집는다. 
    var arr7 = ['a', 'b', 'c', 'd'];
    console.log(arr7.reverse()); // ['d', 'c', 'b', 'a']
}

function TestObject(){
    let obj = {
        name : '홍길동',
        age : 21
    }; // 선언과 동시에 할당
    console.log(obj);

    //console.log(obj + "1"); 플러스 쓰면 오브젝트가 이상하게 나옴
    //console.log(obj, "1");  콤마 쓰면 각각 따로 나옴

    let obj1 = {};
    obj1.name = '홍길동';
    obj1.age = 21;
    console.log(obj1);

    let obj2 = new Object();  // () 안에 있는 데이터 타입으로 객체 생성
    obj2['name'] = '홍길동';
    obj2['age'] = 21;
    console.log(obj2);

    let obj3 = new Object({name: '홍길동', age: 21});
    console.log(obj3);

    let obj4 = new Object('홍길동');
    console.log(obj4);
    // string 객체 생겅
    // [String : '홍길동']
}

function TestObject2(){
    let obj = {
        name : "원숭이",
        age :12,
        favorite : 'banana'
    }

    // 객체의 key와  value를 열거해준다.
    for(let key in obj){
        console.log(key + ':' + obj[key]);
    }
    console.log('------------------------------');

    // of는 값을 나열할때 사용
    // of는 배열에서만 사용, Object에서 쓰면 에러
    let arr = Object.values(obj);
    for(let value of arr){
        console.log(value);
    }
    console.log('------------------------------');

    let arr2  = Object.keys(obj);
    for(let key of arr2){
        console.log(key);
    }
    console.log('------------------------------');
}

//TestArr1();
TestArr3();