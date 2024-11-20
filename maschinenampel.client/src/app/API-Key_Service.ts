import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  readonly Server_URL = "https://localhost:7204/";

  readonly DB_APIUrl = this.Server_URL + "api/DBController/";
  readonly OPC_APIUrl = this.Server_URL + "api/OPCController/";
  readonly uploadUrl = this.Server_URL + "api/imgUpload/upload";
}
