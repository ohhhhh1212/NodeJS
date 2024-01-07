"use strict"

function Test_Dic(){
    let obj ={
        1 : "사과",
        2 : "배",
        3 : "수박"
    }

    Print(obj)
    console.log("------------------------------");

    obj[1] = "맛있는 사과";
    obj[2] = "맛있는 배";
    
    Print(obj);
    console.log("------------------------------");

    let arr = Object.keys(obj);
    for(let i of arr){
        if(i == 1){
            delete obj[i];
        }
    }
    //delete obj[1];

    Print(obj);
}

function Print(obj){
    for(let i in obj){
        console.log(`${i} : ${obj[i]}`);
    }
}

Test_Dic();