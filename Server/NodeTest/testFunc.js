"use strict"

function Add(a, b){
    console.log("sum = ", a + b);
}

function Test_Const2(){
    const car = {
        brand: '기아',
        model: '카니발',
        year: 2006
    }

    console.log("brand = " + car.brand);
    car.brand = '벤츠';
    console.log("brand = " + car.brand);
}

const multiply = new Function('x', 'y', 'return x * y');

const multiply3 = function(x, y){
    return x * y;
}

const multiply4 = function funcName(x, y){
    return x * y;
}

// 람다식
const multiply5 = (x, y) => x * y;
const multiply51 = (x, y) => {
    return x * y;
}

Test_Const2();

module.exports = {
    Add,
    Test_Const2,
    multiply,
    multiply3,
    multiply4,
    multiply5,
    multiply51
}