"use strict"

function MantoMan(name, weight){
    this.name = name;
    this.weight = weight;

    this.getName = function(){
        return this.name;
    }
    this.setName = function(name){
        this.name = name;
    }
}
function Test_Class1(){
    let man = new MantoMan('박승현', 83);
    console.log(man.getName(), man.weight);

    man.setName('권규범');
    console.log(man.getName(), man.weight);
}


class Animal{
    constructor(name, sound){
        this.name = name;
        this.sound = sound;
    }

    Speaker(){
        return this.sound;
    }

    get getName(){
        return this.name;
    }
}
function Test_Class2(){
    const ani = new Animal('호랑이', '어흥');
    console.log(ani);

    console.log(ani.getName, ani.Speaker());
}


class Rectangle{

}



Test_Class2();