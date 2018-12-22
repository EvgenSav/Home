import {  NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { HelloComponent } from './app.component';

@NgModule({
    imports: [BrowserModule], // import Angular's BrowserModule
    bootstrap: [HelloComponent],  // indicate the bootstrap component
    declarations: [HelloComponent] // register our component with the module
})
export class AppModule { }