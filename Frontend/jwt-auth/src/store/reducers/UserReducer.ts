import { createSlice } from '@reduxjs/toolkit'
import { IUserDto } from '../../interfaces/IUserDto'
import { getAllUsers } from '../action-creators/UserActionCreators'

interface UserState {
    users: IUserDto[] | undefined
    isLoading: boolean
    error: string | undefined | unknown
}

const initialState: UserState = {
    users: [],
    isLoading: false,
    error: '',
}

const userSlice = createSlice({
    name: 'user',
    initialState: initialState,
    reducers: {
        clearUsers: (state) => {
            state.users = []
        },
        clearError: (state) => {
            state.error = undefined
        },
    },
    extraReducers: (builder) => {
        builder.addCase(getAllUsers.fulfilled, (state, action) => {
            state.isLoading = false
            state.users = action.payload
            state.error = undefined
        })
        builder.addCase(getAllUsers.pending, (state) => {
            state.isLoading = true
        })
        builder.addCase(getAllUsers.rejected, (state, action) => {
            state.isLoading = false
            state.error = action.payload
            state.users = []
        })
    },
})

export const userActions = userSlice.actions

export default userSlice.reducer
