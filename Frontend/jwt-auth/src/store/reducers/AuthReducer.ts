import { createSlice, PayloadAction } from '@reduxjs/toolkit'
import { IUserData } from '../../interfaces/IUserData'
import { signIn, registration, logout } from '../action-creators/AuthActionCreators'

export interface AuthState {
    userData: IUserData | null
    isLoading: boolean
    error: string | undefined | unknown
    isAuth: boolean
}

const initialState: AuthState = {
    userData: null,
    isLoading: false,
    error: '',
    isAuth: false,
}

const authSlice = createSlice({
    name: 'auth',
    initialState: initialState,
    reducers: {
        refreshPending: (state) => {
            state.isLoading = true
        },
        refreshFulfilled: (state, action: PayloadAction<IUserData>) => {
            state.isLoading = false
            state.userData = action.payload
            state.isAuth = true
            state.error = undefined
        },
        refreshRejected: (state) => {
            state.isLoading = false
            state.userData = null
            state.isAuth = false
        },
        clearError: (state) => {
            state.error = undefined
        },
    },
    extraReducers: (builder) => {
        builder.addCase(registration.fulfilled, (state, action) => {
            state.isLoading = false
            state.userData = action.payload
            state.error = undefined
            state.isAuth = true
        })
        builder.addCase(registration.pending, (state) => {
            state.isLoading = true
        })
        builder.addCase(registration.rejected, (state, action) => {
            state.isLoading = false
            state.error = action.payload
        })
        builder.addCase(signIn.fulfilled, (state, action) => {
            state.isLoading = false
            state.userData = action.payload
            state.isAuth = true
            state.error = undefined
        })
        builder.addCase(signIn.pending, (state) => {
            state.isLoading = true
        })
        builder.addCase(signIn.rejected, (state, action) => {
            state.isLoading = false
            state.error = action.payload
        })
        builder.addCase(logout.fulfilled, (state) => {
            state.isLoading = false
            state.userData = initialState.userData
            state.isAuth = false
            state.error = undefined
        })
        builder.addCase(logout.pending, (state) => {
            state.isLoading = true
        })
        builder.addCase(logout.rejected, (state) => {
            state.isLoading = false
        })
    },
})

export const authActions = authSlice.actions

export default authSlice.reducer
