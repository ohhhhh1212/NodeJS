"use strict"

const tevar = require("./testVar.js");
const testfunc = require("./testFunc.js");

//tevar.Test_Var();

testfunc.Add(5, 9);
testfunc.Add(5);
testfunc.Add(3, 5, 5);
let a = testfunc.multiply(1, 6)
console.log(a);