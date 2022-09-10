import { createAsyncThunk } from '@reduxjs/toolkit'
import { UserService } from '../../services/UserService'

export const getAllUsers = createAsyncThunk('getAllUsers', async (_, thunkApi) => {
    try {
        return await UserService.getAllUsers()
    } catch (error: any) {
        return thunkApi.rejectWithValue(error.response.data)
    }
})
