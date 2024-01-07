"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.Lobby = exports.ERoomstate = void 0;
const UserInfo_1 = __importDefault(require("./UserInfo"));
const Room_1 = __importDefault(require("./Room"));
//import RoomPlayer from "./RoomPlayer";
const UserStorage_1 = require("./UserStorage");
const storage = new UserStorage_1.UserStorage(); // 저장 리스트 정보
const DLOGIN_SUCCESS = 0; // 로그인 성공
const DLOGIN_NOT_ID = 1; // 존재하지 않는 ID
const DLOGIN_WRONG_PASS = 2; // 패스워드 다름
const DLOGIN_ALREADY_LOGIN = 3; // 이미 로그인중이다.
//룸 상태 
exports.ERoomstate = {
    Ready: 0, // 대기상태 ( 입장가능 )
    Game: 1 // 게임상태 ( 입장불가 )
};
;
//
//   로비
//
class Lobby {
    constructor() {
        // 계정생성 응답 -----------------------------------------
        this.Ack_CreateId = (socket, id, pass) => {
            const resUser = storage.FindUserData(id);
            if (resUser != undefined) {
                const packet = { 'id': id, 'success': 1, 'socketId': socket.id }; // 이미 가입된 유저임
                socket.emit("ack_create_id", packet);
                return;
            }
            //유저정보 추가
            storage.PushData(id, pass);
            // 파일 저장
            storage.SaveFile();
            console.log('<create_id>\n', storage);
            const nRes = 0;
            const packet = { 'id': id, 'success': nRes, 'socketId': socket.id }; // 정상 가입됨
            socket.emit("ack_create_id", packet);
        };
        // 룸 생성 응답 ----------------------------------------
        this.Ack_CreateRoom = (socket, roomName, userId) => {
            if (!(userId in this.lobbyUserList)) {
                console.log("create_room.. ", `없는 유저이다. id = ${userId}`);
                return;
            }
            //socket.group(roomName);
            socket.join(roomName);
            const room = new Room_1.default(roomName, userId);
            const user = this.lobbyUserList[userId];
            room.Initialize(user);
            this.roomList[roomName] = room;
            console.log("create_room.. ", this.roomList[roomName]);
            const packet = { "success": 0, "room": room };
            socket.emit("ack_create_room", packet);
            this.SendUpdateRoomList(socket);
            // var packet2 = {"datas": Object.values(roomList)};
            // socket.broadcast.emit('notify_update_roomlist', packet2);
        };
        // 룸 조인 응답 ----------------------------------------
        this.Ack_JoinRoom = (socket, roomName, userId) => {
            //1. 룸이 없어 조인 실패 ( fail code : 1 )
            if (!(roomName in this.roomList)) {
                console.log("join_room.. ", `없는 Room이다. name = ${roomName}`);
                const room = new Room_1.default(roomName, userId);
                socket.emit("ack_join_room", { "success": 1, "room": room }); // 조인 실패
                return;
            }
            const kRoom = this.roomList[roomName];
            //2. 게임중이면 접속불가 통지하기 ( fail code : 2 )
            if (kRoom.roomState == exports.ERoomstate.Game) {
                const packet = { "success": 2, "room": kRoom };
                socket.emit("ack_join_room", packet);
                return;
            }
            //3. 최대 플레이어 체크하기 ( fail code : 3 )
            if (kRoom.players.length >= kRoom.maxPlayer) {
                const packet = { "success": 3, "room": kRoom };
                socket.emit("ack_join_room", packet);
                return;
            }
            //4. 같은유저 접속 체크 ( fail code : 4 )  
            let kSamePlayer = kRoom.players.find((e) => (e.Name() == userId));
            if (kSamePlayer != undefined) {
                const packet = { "success": 4, "room": kRoom };
                socket.emit("ack_join_room", packet);
                return;
            }
            // 조인성공 
            socket.join(roomName);
            kRoom.removedFromList = false;
            const kUserInfo = this.lobbyUserList[userId];
            const kPlayer = kRoom.AddPlayer(kUserInfo);
            //console.log('room list = ', io.adapter.rooms);
            const packet = { "success": 0, "room": kRoom };
            socket.emit("ack_join_room", packet); // socket 유저에게 룸정보 보내기
            socket.broadcast.to(roomName).emit('notify_enter_room', kPlayer); // 다른 유저에게 socket user 정보 보내기
            //io.to(roomName).emit('enter_room', kPlayer);        // 모든 유저에게 socket user 정보 보내기
            this.SendUpdateRoomList(socket);
        };
        this.lobbyUserList = {}; // dictioary로 사용, 로비에 접속된 전체 유저 리스트 
        this.roomList = {}; // 룸리스트
    }
    // 유저 데이타 초기화 하기 ( 가입유저 파일에서 열기 ) -----------
    Init_UserData() {
        storage.LoadFile();
    }
    // 로그인 응답 -----------------------------------------
    Ack_Login(socket, id, pass) {
        //조건체크
        const nRes = this.CheckLogin(id, pass);
        if (nRes == 0)
            console.log(`<login> Login Success ★ ★ ,  ( id : ${id} ) `);
        else
            console.log(`<login> Login Failed ☎ ☎,  code : ${nRes}`);
        // 성공여부 클라이언트에 전송
        const packet = { 'id': id, 'success': nRes };
        socket.emit('ack_login', packet);
    }
    // 유저정보 응답 (클라이언트에서 로그인 성공하면 호출해야 됨)----------------
    Ack_LobyUserInfo(socket, data) {
        const info = JSON.parse(data);
        // 유저가 존재하는지 체크
        if ((info.id in this.lobbyUserList)) {
            return;
        }
        // dataPoat, movePort가 겹치면 +2씩 계속 증가시키기( while문 이용 )
        const list = Object.values(this.lobbyUserList);
        list.sort((a, b) => (a.dataPort - b.dataPort));
        for (var user of list) {
            if (user.ip == info.ip) {
                if (user.dataPort == info.dataPort)
                    info.dataPort += 1;
                if (user.movePort == info.movePort)
                    info.movePort += 1;
            }
        }
        // 새로운 유저정보 등록
        const newUser = new UserInfo_1.default(info.id, socket.id, info.ip, info.dataPort, info.movePort);
        this.lobbyUserList[info.id] = newUser;
        console.log(`lobbyUserList count = ${Object.keys(this.lobbyUserList).length}\n`); //, lobbyUserList);
    }
    // 로그아웃 응답 ----------------------------------------
    Ack_Logout(socket, id) {
        console.log('로그아웃', id, socket.id);
        //clearInterval( socket.interval );
        //로그아웃정보를 모든유저에게 보낸다.
        const packet = { 'id': id };
        socket.broadcast.emit('notify_logout', packet);
        //목록에서 삭제
        this.LogoutUser(socket, id);
    }
    // 연결해제시 응답 ----------------------------------------
    Ack_Disconnect(socket) {
        return __awaiter(this, void 0, void 0, function* () {
            const socketId = socket.id;
            console.log('<disconnect> 클라이언트 접속 해제 : socket id =', socket.id);
            //clearInterval(socket.interval);  // Ping 타이머 해제
            const userId = this.FindUserBySocketId(socket.id);
            //유저가 룸에 속해있으면 룸에서 삭제
            if (userId != undefined) {
                var kRoom = this.FindRoomByUserId(userId); // 유저가 속해있는 룸 검색
                if (kRoom != undefined) {
                    kRoom.RemovePlayer(userId); // 룸에서 유저 삭제
                    kRoom.SendLeaveRoomPlayer(socket, userId); // 룸 유저 삭제 - 모든유저에게 전송
                    if (kRoom.PlayerCount() == 0) {
                        kRoom.removedFromList = true;
                        yield socket.broadcast.emit('notify_update_roomlist', { 'datas': Object.values(this.roomList) });
                        delete this.roomList[kRoom.Name()];
                    }
                    else {
                        this.SendUpdateRoomList(socket); //로비유저의 룸리스트 정보 갱신
                    }
                }
            }
            this.LogoutUser(socket, userId);
            console.log('user count =', Object.keys(this.lobbyUserList).length);
        });
    }
    // 가입탈퇴시 응답 -----------------------------------------
    Ack_Withdraw(socket, id) {
        if (storage.RemoveUserData(id) == true)
            storage.SaveFile();
    }
    // 로그아웃 후 처리 ( param : 유저 id ) ----------------
    LogoutUser(socket, id) {
        if (id in this.lobbyUserList) {
            delete this.lobbyUserList[id];
        }
    }
    // 로비채팅 응답 --------------------------------------------
    Ack_LobbyChat(socket, id, msg) {
        console.log('채팅요청', id, msg);
        //요청에 대한 응답 보내기
        let packet = { id: id, msg: msg };
        socket.emit('ack_lobby_chat', packet);
        // chat msg를 본인을 제외한 모든유저에게 보낸다.
        //let packet2 = {id:id, msg:msg};
        socket.broadcast.emit('notify_lobby_chat', packet);
    }
    // 룸 리스트 응답 ---------------------------------------
    Ack_InitRoomList(socket, userId) {
        console.log("room list count = ", Object.keys(this.roomList).length);
        const packet = { "datas": Object.values(this.roomList) };
        socket.emit('ack_init_roomlist', packet);
    }
    // 룸 나가기 ------------------------------------------
    Ack_LeaveRoom(socket, roomName, userId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield socket.broadcast.to(roomName).emit('notify_leave_room', { "id": userId });
            //io.to(roomName).emit('leave_room', {"id": userId} );
            socket.leave(roomName);
            const kRoom = this.roomList[roomName];
            kRoom.RemovePlayer(userId);
            console.log("Leave room user id = ", userId);
            console.log("room player count = ", kRoom.PlayerCount());
            //로비에 있는 유저들에게 알림
            if (kRoom.PlayerCount() == 0) {
                kRoom.removedFromList = true;
                yield socket.broadcast.emit('notify_update_roomlist', { 'datas': Object.values(this.roomList) });
                delete this.roomList[roomName];
            }
            else {
                //마스터가 방을 나가면 방장 인계
                if (kRoom.masterClientId == userId) {
                    let kPlayer = kRoom.FirstPlayer();
                    if (kPlayer == undefined)
                        return;
                    kRoom.masterClientId = kPlayer.Name();
                    kPlayer.isMaster = true;
                    console.log('master id = ', kPlayer.Name());
                    const changePacket = { "masterId": kPlayer.Name() };
                    yield socket.broadcast.to(roomName).emit('notify_room_change_master', changePacket);
                }
                this.SendUpdateRoomList(socket);
            }
            0;
        });
    }
    // 룸에서 Ready 상태 응답 --------------------------------
    Ack_RoomUserReady(socket, roomName, userId, userState) {
        const kRoom = this.roomList[roomName];
        const kPlayer = kRoom.FindPlayer(userId);
        if (kPlayer != undefined)
            kPlayer.userState = userState;
        console.log(userId, 'State : ', userState);
        socket.broadcast.to(roomName).emit('notify_room_ready', { "id": userId, "userState": userState });
    }
    // 게임 시작 ------------------------------------------
    Ack_GameStart(socket, roomName, userId) {
        const room = this.roomList[roomName];
        room.roomState = exports.ERoomstate.Game;
        if (room.masterClientId == userId) {
            //io.sockets.in(roomName).emit('notify_game_start', {"start": true} ); // 나를 제외한 룸의 모든 유저에게 보냄
            socket.broadcast.to(roomName).emit('notify_game_start', { "start": true }); // 나를 제외한 룸의 모든유저에게 보냄
        }
        this.SendUpdateRoomList(socket);
    }
    // 게임 종료 --------------------------------------------
    Ack_GameEnd(socket, roomName, userId) {
        const room = this.roomList[roomName];
        room.roomState = exports.ERoomstate.Ready;
        if (room.masterClientId == userId) {
            socket.broadcast.to(roomName).emit('notify_game_End', { "end": true }); // 나를 제외한 룸의 모든유저에게 보냄
        }
        this.SendUpdateRoomList(socket);
    }
    // 룸리스트 정보 업데이트 하기
    SendUpdateRoomList(socket) {
        const packet = { "datas": Object.values(this.roomList) };
        socket.broadcast.emit('notify_update_roomlist', packet);
    }
    // 로그인 체크하기 --------------------------------
    CheckLogin(id, pass) {
        const findUser = storage.FindUserData(id);
        // 존재하지 않은 id
        if (findUser == undefined)
            return DLOGIN_NOT_ID;
        // 패스워드 다름
        if (findUser.pass != pass)
            return DLOGIN_WRONG_PASS;
        // 이미 로그인 중이다.   
        if (findUser.id in this.lobbyUserList)
            return DLOGIN_ALREADY_LOGIN;
        // 성공
        return DLOGIN_SUCCESS;
    }
    // 유저id로 유저 찾기
    FindUser(id) {
        if (id in this.lobbyUserList)
            return this.lobbyUserList[id];
        return undefined;
    }
    // 소켓id로 유저 Id 찾기
    FindUserBySocketId(socketId) {
        const findUser = Object.values(this.lobbyUserList).find((element, idx) => {
            return (element.socketId == socketId);
        });
        if (findUser != undefined)
            return findUser.id;
        return "";
    }
    // 룸안에 있는 유저의 룸 찾기
    FindRoomByUserId(userId) {
        for (var key in this.roomList) {
            var kRoom = this.roomList[key];
            var kPlayer = kRoom.FindPlayer(userId);
            if (kPlayer != undefined)
                return kRoom;
        }
        return undefined;
    }
}
exports.Lobby = Lobby;
exports.default = Lobby;
