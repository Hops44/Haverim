import React from "react";
import "../css/ImageUploadModal.css";
import { ImageCropper } from "./RegisterPage";
import { uploadImage } from "../GlobalRequests";

export class ImageUploadModal extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      profileImage: false
    };
  }
  render() {
    return (
      <React.Fragment>
        <div onClick={this.props.closeFunction} className="image-upload-modal-background" />
        <div className="image-upload-modal-body">
          {this.state.profileImage === false ? (
            <React.Fragment>
              <div
                onClick={e => {
                  let input = document.createElement("input");
                  input.setAttribute("type", "file");
                  input.accept = "image/*";
                  input.click();
                  input.oninput = function(e) {
                    let fr = new FileReader();
                    fr.onload = function(e) {
                      this.setState({
                        profileImage: e.srcElement.result
                      });
                    }.bind(this);
                    fr.readAsDataURL(e.target.files[0]);
                  }.bind(this);
                }}
                className="image-upload-modal-picture-upload"
              >
                <div className="image-upload-modal-picture-upload-center">
                  <img
                    draggable={false}
                    className="image-upload-modal-picture-upload-icon"
                    src="/Assets/upload-picture.svg"
                  />
                  <p className="image-upload-modal-picture-upload-text noselect">Click to upload</p>
                </div>
              </div>
            </React.Fragment>
          ) : (
            <div className={"image-upload-modal-container"}>
              <ImageCropper
                ratio={this.props.type === 0 ? 1 : 0}
                classNamePrefix={"image-upload-modal"}
                updateCroppedValue={data => this.setState({ profileImage: data })}
                resetImage={() => this.setState({ profileImage: false })}
                imageData={this.state.profileImage}
                finishFunction={() =>
                  uploadImage(this.state.profileImage, this.props.type).then(value =>
                    this.props.reloadFunction()
                  )
                }
              />
            </div>
          )}
        </div>
      </React.Fragment>
    );
  }
}
