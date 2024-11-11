import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UploadImageComponent } from "../upload-image/upload-image.component";
import { CommonModule } from '@angular/common';
import { RegisterUserDTO } from '../Model/DTO/RegisterUserDTO';
import { HttpClient, HttpErrorResponse, HttpClientModule } from '@angular/common/http';
import { UserDAO } from '../Model/DAO/UserDAO';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, UploadImageComponent, CommonModule, HttpClientModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {

  private registerForm!: FormGroup;
  private selectedImage: File | null = null;
  private userDAO: UserDAO;
  private confirmPwdIsGood: boolean;
  private isStrongPassword: boolean;

  public get IsStrongPassword(): boolean {
    return this.isStrongPassword;
  }

  public get ConfirmPwdIsGood(): boolean {
    return this.confirmPwdIsGood;
  }

  public get RegisterForm(): FormGroup {
    return this.registerForm;
  }

  public set RegisterForm(value: FormGroup) {
    this.registerForm = value;
  }

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    this.userDAO = new UserDAO(this.http);
    this.confirmPwdIsGood = true;
    this.isStrongPassword = false;
  }

  public ngOnInit(): void {
    this.registerForm = this.fb.group({
      pseudo: ['', Validators.required],
      password: ['', [Validators.required, Validators.pattern("^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$")]],
      email: ['', [Validators.required, Validators.email]],
      confirmPassword: ['', Validators.required],
      img: [null]
    });
    this.InitializePwdForm();
  }

  /**
   * Gère la soumission du formulaire d'inscription
   */
  public onSubmit(): void {
    this.InitializePwdForm();

    if (this.registerForm.valid && this.registerForm.value.password === this.registerForm.value.confirmPassword) {
      const registerUserDTO = new RegisterUserDTO(
        this.registerForm.value.pseudo,
        this.registerForm.value.email,
        this.registerForm.value.password,
        this.selectedImage
      );
      this.userDAO.registerUser(registerUserDTO).subscribe({
        next: (response: { message: string }) => {
          this.router.navigate(['/login']).then(() => {
            // Affiche un message de succès et redirige vers la page de connexion 
            const Toast = Swal.mixin({
              toast: true,
              position: "top-end",
              showConfirmButton: false,
              timer: 5000,
              timerProgressBar: true
            });
            Toast.fire({
              icon: "success",
              title: "Inscription réussie, connectez-vous !",
            });
          });
        },
        error: (err: HttpErrorResponse) => {
          Swal.fire({
            icon: 'error',
            title: 'Erreur lors de l\'inscription',
            text: err.message,
            showConfirmButton: true
          });
        }
      });

    } else {
      let errorMessage = 'Formulaire non valide. Veuillez corriger les erreurs.';
      if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
        this.confirmPwdIsGood = false;
      }
      if (this.registerForm.get('password')?.hasError('pattern')) {
        this.isStrongPassword = false;
        errorMessage = 'Mot de passe trop faible. Veuillez respecter les critères.';
      }
      Swal.fire({
        icon: 'error',
        title: 'Erreur',
        text: errorMessage,
        showConfirmButton: true,
        showCloseButton: true,
        customClass: {
          confirmButton: 'custom-ok-button'
        },
      });
    }
  }

  public onImageSelected(image: File) {
    this.selectedImage = image;
    this.registerForm.patchValue({ img: this.selectedImage });
  }

  public InitializePwdForm(): void {
    this.confirmPwdIsGood = true;
    this.isStrongPassword = false;
  }
}
