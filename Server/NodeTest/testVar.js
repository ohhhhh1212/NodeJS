"use strict"


// var, let, const 
function Test_Var(){
    var a = 10;

    if(a > 9){
        var a = 30;
        console.log("a = " + a);
    }

    console.log("a = " + a);
}

function Test_let(){
    let a = 10;

    if(a > 9){
        let a = 30;
        console.log("a = " + a);
    }

    console.log("a = " + a);
}

module.exports = {

    Test_Var,
    Test_let
}

//Test_Var();