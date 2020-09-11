import { Component } from '@angular/core';
import { CoordSystemService } from './coord-system.service';
import { CoordinateSystemFile } from './coord-system.-model';

@Component({
  selector: 'coord-system',
  templateUrl: './coord-system.component.html',
  providers: [CoordSystemService],
  styleUrls: ['./coord-system.component.css']
})

export class CoordSystemComponent {
  private projectUid: string = localStorage.getItem("projectUid");

  public coordSystemSettings: any;

  constructor(private coordSystemService: CoordSystemService) { }
  ngOnInit() {
    var self = this;

    this.coordSystemService.getCoordSystemSettings(this.projectUid).subscribe((settings: string) => self.coordSystemSettings = settings);
  }

  public addUpdateCoordSystem(files: FileList): void {
    const stringSeparator = "base64,";
    const reader = new FileReader();

    reader.readAsDataURL(files[0]);    

    reader.onload = () => {
      var self = this;

      const csFileContent = reader.result.toString().split(stringSeparator)[1];
      const csFile = new CoordinateSystemFile(this.projectUid, csFileContent, files[0].name);

      this.coordSystemService.addUpdateCoordSystem(csFile).subscribe((settings: string) => self.coordSystemSettings = settings);
    };
  }
}