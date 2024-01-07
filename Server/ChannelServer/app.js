"use strict"

const app = require("express");
const server = require('http').createServer(app);
const io = require('socket.io')(server, {
    pingTimeout : 20000,    // 핑 연결이 실패후 20초 후 연결 해제한다.
    pingInterval: 15000     // 15초 간격으로 핑을 보낸다.
});

const file = require("fs");


const FILENAME = "users.json";
const storage = {};
const userList = {};  // dictioary로 사용


// 클래스의 사용은 오브젝트 보다 실수 할 확률이 줄어든다.
class UserInfo {
  constructor( id, socketId, ip, dataPort, backPort) {
    this.id = id;
    this.socketId = socketId;
    this.ip = ip;
    this.dataPort = dataPort;
    this.backPort = backPort;
  }
}


storage.datas = [];

io.use((socket, next) => {
  if (socket.handshake.query.token === "UNITY") {
      next();
  } else {
      next(new Error("Authentication error"));
  }
});

Init_UserData();

io.on('connection', (socket) => {
  //content
  const req = socket.request;
  const ip = req.headers['x-forwarded-for'] || req.connection.remoteAddress;  
   console.log(`<connection> 클라이언트 접속, socket id = ${socket.id},  ip = ${ip}` );
  //console.log('클라이언트 접속 - ', ip, socket.id, req.ip);
   
  /// Join 응답-------------------------
  socket.on('join', (id, pass)=> {
    Ack_Join( socket, id, pass );
  });

  // Login 응답 -------------------------
  socket.on('login', (id, pass) => {
    Ack_Login(socket, id, pass);
  });
  
  // 유저정보 받아서 다른유저에게 브로드캐스팅하기 ----
  socket.on('user_info', (data) => {
    Ack_UserInfo(socket, data);  
  });

  // logout 응답 -------------------------
  socket.on('logout', (id) =>{
    Ack_Logout(socket, id);
  });

  // 가입 탈퇴 응답 -------------------------
  socket.on('withdraw', (id) =>{
    Ack_Withdraw(socket, id);
  });

  


  // 필요없음... 서버에서 클라이언트로 ping을 엔진에서 보낸다.
//  socket.on('ping', (time)=>{
//     console.log(`.. : ${time}`);
//  });

  // 연결 종료 시 -------------------------
  socket.on('disconnect', async () => {
    Ack_Disconnect(socket);
  });

});

// 유저 데이타 초기화 하기 ( 가입유저 파일에서 열기 ) -----------
function Init_UserData() {
  try {
    file.readFile(FILENAME, 'utf-8', (err, data) =>{
      if( err ) {
        console.log(err);
      }
      else {
        if( data != "") {
          var storage2 = JSON.parse(data);  
          if( storage2.hasOwnProperty("datas")) {
            storage.datas = storage2.datas;
          }
          console.log('<Init_UserData>\n',storage);
        }
      }
    });
  }
  catch(err){
    console.log(err);
    //throw err;
  } 
}

// 조인 응답 (채널서버에 가입)-----------------------------------------
const Ack_Join =(socket, id, pass )=>{
  
  const resUser = storage.datas.find((element)=>{
     if( element.id == id )
        return true; 
      return false;
  });

  if( resUser != undefined ){
    var packet = {id:id, success: 1, socketId:socket.id}   // 이미 가입된 유저임
    socket.emit("join", packet);
    return;
  }

  storage.datas.push( {id:id, pass:pass} );

  // 파일 저장
  SaveUserFile();

  console.log('<join>\n', storage);
  var nRes = 0;
  var packet = {id:id, success: nRes, socketId:socket.id};  // 정상 가입됨
  socket.emit("join", packet);   
}

// 로그인 응답 -----------------------------------------
function Ack_Login (socket, id, pass) {
    // var data = {"id":id, "pass":pass};
    // console.log(data);
    //조건체크
    const nRes = CheckLogin( id, pass); 

    if( nRes == 0 ) 
      console.log(`<login> Login Success ★★ ,  ( id : ${id} ) `);
    else
      console.log(`<login> Login Failed ☎☎,  code : ${nRes}`);

    // 성공여부 클라이언트에 전송
    const packet = {id:id, success:nRes};
    socket.emit('login', packet);
}

// 유저정보 응답 ---------------------------------------
function Ack_UserInfo(socket, data) {
    const info = JSON.parse(data);   // UserInfo로 파싱하기
        
    // 유저가 존재하는지 체크
    if( (info.id in userList ) ) {
      return;
    }
    // dataPoat, backPort가 겹치면 +2씩 계속 증가시키기( while문 이용 )
    var list = Object.values(userList);
    for( var user of list ) {
      if( user.dataPort == info.dataPort ) 
          info.dataPort += 2;
      
      if( user.backPort == info.backPort )
          info.backPort += 2;
    }
    // 새로운 유저정보 등록
    var newUser = new UserInfo();
    //var newUser = {};
    newUser.id = info.id;
    newUser.socketId = socket.id;
    newUser.ip = info.ip;
    newUser.dataPort = info.dataPort;
    newUser.backPort = info.backPort;
    userList[info.id] = newUser;

    // 보낸유저 제외하고 모두에게 새로운 유저정보 보내기
    socket.broadcast.emit("user_info", newUser) ;
    console.log(`<user_info> count = ${Object.keys(userList).length}\n`, userList);

    //new 유저에게 userList 전부 보내기
    socket.emit('user_info_list', {datas: Object.values(userList)}); 
}

// 로그아웃 응답 ----------------------------------------
function Ack_Logout(socket, id) {
  console.log('로그아웃', id, socket.id);
  clearInterval(socket.interval);
  
  //로그아웃정보를 모든유저에게 보낸다.
  const packet = {id:id};
  socket.broadcast.emit('logout', packet );
  
  //목록에서 삭제
  LogoutUser( id );
}

// 연결해제시 응답 ----------------------------------------
function Ack_Disconnect(socket) {
  const socketId = socket.id;
  console.log('<disconnect> 클라이언트 접속 해제 : socket id =', socket.id);
  clearInterval(socket.interval);
  const userId = FindUserBySocketId(socket.id);
  //console.log(userId);
  LogoutUser( userId );
  console.log('user count =', Object.keys(userList).length);
}
// 가입탈퇴시 응답 -----------------------------------------
function Ack_Withdraw(socket, id) {
  var index = -1;
  storage.datas.find( (user, idx) => {
    if( user.id == id){
      index = idx;
      return true;
    }
    return false;
  });

  if( index != -1) {
    storage.datas.splice(index, 1);  // 찾은 유저 삭제
    SaveUserFile();
  }
   console.log(`<withdraw> : id=${id}, idx=${index}`); 
}

// 로그아웃 후 처리 ( param : 유저 id ) ----------------
function LogoutUser( id ) {
  if( id in userList ) {
    delete userList[id];
  }
}

// 파일 저장하기 --------------------------------------
function SaveUserFile() {
  file.writeFile(FILENAME, JSON.stringify(storage), (err)=>{
    if( err ) {
      console.log(err);
      //throw err;
    }
  });
}


const DLOGIN_SUCCESS = 0;     // 로그인 성공
const DLOGIN_NOT_ID = 1;      // 존재하지 않는 ID
const DLOGIN_WRONG_PASS = 2;  // 패스워드 다름
const DLOGIN_ALREADY_LOGIN = 3; // 이미 로그인중이다.

// 로그인 체크하기 --------------------------------
function CheckLogin(id, pass) {

  var index = -1;
  // id가 없음
  const findUser = storage.datas.find((element, idx) =>{
      if(element.id == id) {
          index = idx;
          return true;
      }
      return false; 
   });

   // 존재하지 않은 id
   if( findUser == undefined)      return DLOGIN_NOT_ID;
     // 패스워드 다름
   if( findUser.pass != pass )     return DLOGIN_WRONG_PASS;
   // 이미 로그인 중이다.   
   if( findUser.id in userList )    return DLOGIN_ALREADY_LOGIN;

  // 성공
  return DLOGIN_SUCCESS;
}


// 유저id로 유저 찾기
function FindUser( id ) {
  if( id in userList )
    return userList[id];

  return undefined;
}

// 소켓id로 유저 Id 찾기
function FindUserBySocketId( socketId ) {
   const findUser = Object.values(userList).find( (element, idx) => {
      if(element.socketId == socketId) {
          return true;
      }
      return false; 
   });
   if( findUser != undefined)
      return findUser.id;

   return undefined;
}


const port = 19000;

server.listen(port, ()=>{
    console.log(`서버 가동 - Port : ${port}`);
});

// io.listen(port);
// console.log('listening on *:' + port);


module.exports = app;



// // 저장 유저리스트를 필드별 얻기 ------------------
// const getSaveUser = ( isAll, ...fields )=>{
//   if( isAll )
//     return storage;

//    const newUsers = fields.reduce(( newUsers, field) =>{
//       if( storage.hasOwnProperty(field)) {
//         newUsers[field] = storage[field];
//       }
//       return newUsers;
//    }, {});

//    return newUsers;
// };
