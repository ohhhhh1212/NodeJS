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
exports.Room = void 0;
//const RoomPlayer = require('./RoomPlayer')
const RoomPlayer_1 = __importDefault(require("./RoomPlayer"));
//룸에서 유저상태 enum 상수
const ERoomUserSate = {
    Empty: 0, // 빈슬롯
    Enter: 1, // 입장된 상태
    Ready: 2, // 준비된 상태
};
//룸 정보
class Room {
    constructor(roomId, masterClientId, maxPlayer = 4) {
        this.roomId = roomId; // 룸id 또는 룸 이름
        this.masterClientId = masterClientId; // 마스터 클라이언트 id
        this.maxPlayer = maxPlayer; // 최대 플레이어 수
        this.isOpen = true; // true: 입장가능, false: 입장불가능(룸리스트에서 숨기기)
        this.roomState = 0; // 0 : 대기중 , 1 : 게임중   
        this.removedFromList = false; //룸 리스트에서 삭제 되었는지 여부
        this.players = []; // 룸 유저리스트 (RoomPlayer 리스트)
    }
    Name() { return this.roomId; }
    // Player 숫자
    PlayerCount() {
        return this.players.length;
    }
    // 생성시 마스터 플레이어 정보 셋팅
    Initialize(userInfo) {
        const kPlayer = new RoomPlayer_1.default(true, true);
        kPlayer.userInfo = userInfo;
        kPlayer.slotNo = this.players.length + 1;
        kPlayer.userState = ERoomUserSate.Enter; // 1 = Enter_State
        this.players.push(kPlayer);
        this.roomState = 0;
        this.removedFromList = false;
    }
    AddPlayer(userInfo) {
        const kPlayer = new RoomPlayer_1.default(false, true);
        kPlayer.userInfo = userInfo;
        kPlayer.slotNo = this.players.length + 1; // 슬롯번호는 1부터 시작한다.
        kPlayer.userState = ERoomUserSate.Enter; // 1 = Enter_State
        this.players.push(kPlayer);
        return kPlayer;
    }
    // Player 삭제
    RemovePlayer(userId) {
        var idx = this.players.findIndex((data) => { return data.Name() == userId; });
        if (idx != -1) {
            this.players.splice(idx, 1);
        }
    }
    // Player 찾기
    FindPlayer(userId) {
        const kPlayer = this.players.find((e) => {
            return (e.Name() == userId);
        });
        return kPlayer;
    }
    FirstPlayer() {
        if (this.players.length > 0)
            return this.players[0];
        return undefined;
    }
    // 룸 나간 유저 브로드 캐스팅하기
    SendLeaveRoomPlayer(socket, userId) {
        return __awaiter(this, void 0, void 0, function* () {
            yield socket.broadcast.to(this.roomId).emit('notify_leave_room', { "id": userId });
        });
    }
}
exports.Room = Room;
exports.default = Room;
