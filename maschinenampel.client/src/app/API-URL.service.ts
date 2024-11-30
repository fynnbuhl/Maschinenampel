import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  readonly DB_APIUrl = environment.serverUrl + "api/DBController/";
  readonly OPC_APIUrl = environment.serverUrl + "api/OPCController/";
  readonly uploadUrl = environment.serverUrl + "api/imgUpload/upload";
}
