//서버에 연결된 유정 기본 정보
class UserInfo {
    constructor( 
                  public id:string, 
                  public socketId:string, 
                  public ip : string, 
                  public dataPort: number, 
                  public movePort: number) 
    {
      this.id = id;
      this.socketId = socketId;
      this.ip = ip;
      this.dataPort = dataPort;
      this.movePort = movePort;
    }

    SetInfo( info : UserInfo) : void {
      this.id = info.id;
      this.socketId = info.socketId;
      this.ip = info.ip;
      this.dataPort = info.dataPort;
      this.movePort = info.movePort;
    }
  }

  export default UserInfo;
  //module.exports = UserInfo;