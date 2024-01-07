"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
//서버에 연결된 유정 기본 정보
class UserInfo {
    constructor(id, socketId, ip, dataPort, movePort) {
        this.id = id;
        this.socketId = socketId;
        this.ip = ip;
        this.dataPort = dataPort;
        this.movePort = movePort;
        this.id = id;
        this.socketId = socketId;
        this.ip = ip;
        this.dataPort = dataPort;
        this.movePort = movePort;
    }
}
exports.default = UserInfo;
//module.exports = UserInfo;
